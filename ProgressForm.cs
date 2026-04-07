using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Openize.Heic.Decoder;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

namespace HeicToJpgShell
{
    public partial class ProgressForm : Form
    {
        [ThreadStatic]
        private static bool isResolving = false;

        static ProgressForm()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                if (isResolving) return null;

                try
                {
                    isResolving = true;
                    string name = new AssemblyName(args.Name).Name;

                    string folderPath = Path.GetDirectoryName(typeof(ProgressForm).Assembly.Location);
                    string assemblyPath = Path.Combine(folderPath, name + ".dll");

                    if (File.Exists(assemblyPath))
                    {
                        return Assembly.LoadFrom(assemblyPath);
                    }
                }
                finally
                {
                    isResolving = false;
                }
                return null;
            };
        }

        private readonly List<string> _heicFiles;
        private readonly int _totalFiles;
        private int _processedCount = 0;
        private int _successCount = 0;
        private int _failureCount = 0;
        private int _skipCount = 0;
        private Stopwatch _totalSw;

        private enum OverwriteMode { Prompt, OverwriteAll, SkipAll }
        private OverwriteMode _overwriteMode = OverwriteMode.Prompt;
        private readonly object _dialogLock = new object();

        public ProgressForm(IEnumerable<string> selectedPaths)
        {
            InitializeComponent();

            // .heic 파일만 필터링
            _heicFiles = selectedPaths
                .Where(p => p.EndsWith(".heic", StringComparison.OrdinalIgnoreCase))
                .ToList();

            _totalFiles = _heicFiles.Count;
            progressBar1.Maximum = _totalFiles;

            // ListView 컬럼 설정 (L10n 적용)
            listView1.Columns.Add(L10n.Get("ColFileName"), 200);
            listView1.Columns.Add(L10n.Get("ColStatus"), 100);
            listView1.Columns.Add(L10n.Get("ColTime"), 100);

            // 초기 아이템 추가
            foreach (var file in _heicFiles)
            {
                var item = new ListViewItem(Path.GetFileName(file));
                item.SubItems.Add(L10n.Get("StatusWaiting"));
                item.SubItems.Add("-");
                listView1.Items.Add(item);
            }

            this.Shown += ProgressForm_Shown;
        }

        private async void ProgressForm_Shown(object sender, EventArgs e)
        {
            if (_totalFiles == 0)
            {
                MessageBox.Show(L10n.Get("NoFilesMsg"));
                btnClose.Enabled = true;
                return;
            }

            UpdateStatus();
            _totalSw = Stopwatch.StartNew();

            await Task.Run(() => ProcessFiles());

            _totalSw.Stop();
            ShowSummary();

            btnClose.Enabled = true;
        }

        private void UpdateStatus()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)UpdateStatus);
                return;
            }

            string statusText = L10n.Format("ProgressLabel", _processedCount, _totalFiles, _successCount, _failureCount, _skipCount);
            lblStatus.Text = statusText;
            this.Text = $"{L10n.Get("FormTitle")} - {_processedCount * 100 / _totalFiles}%";
        }

        private void ShowSummary()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)ShowSummary);
                return;
            }

            string message = L10n.Format("SummaryBody", _totalFiles, _successCount, _failureCount, _skipCount, _totalSw.Elapsed.TotalSeconds.ToString("F2"));

            MessageBox.Show(message, L10n.Get("TaskComplete"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ProcessFiles()
        {
            int maxParallel = Math.Min(Environment.ProcessorCount, 4);
            using (var semaphore = new SemaphoreSlim(maxParallel))
            {
                var tasks = _heicFiles.Select(async (filePath, index) =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var listViewItem = listView1.Items[index];
                        string outputJpg = Path.ChangeExtension(filePath, ".jpg");

                        this.Invoke((MethodInvoker)delegate
                        {
                            listViewItem.SubItems[1].Text = L10n.Get("StatusConverting");
                            listViewItem.ForeColor = Color.Blue;
                        });

                        var sw = Stopwatch.StartNew();
                        try
                        {
                            // 덮어쓰기 로직 체크
                            bool shouldConvert = true;
                            if (File.Exists(outputJpg))
                            {
                                shouldConvert = CheckOverwrite(Path.GetFileName(outputJpg));
                            }

                            if (shouldConvert)
                            {
                                await Task.Run(() => ConvertHeicToJpg(filePath, outputJpg));
                                sw.Stop();

                                Interlocked.Increment(ref _successCount);
                                this.Invoke((MethodInvoker)delegate
                                {
                                    listViewItem.SubItems[1].Text = L10n.Get("StatusSuccess");
                                    listViewItem.SubItems[2].Text = $"{sw.Elapsed.TotalSeconds:F2}s";
                                    listViewItem.ForeColor = Color.Green;
                                });
                            }
                            else
                            {
                                sw.Stop();
                                Interlocked.Increment(ref _skipCount);
                                this.Invoke((MethodInvoker)delegate
                                {
                                    listViewItem.SubItems[1].Text = L10n.Get("StatusSkipped");
                                    listViewItem.SubItems[2].Text = "-";
                                    listViewItem.ForeColor = Color.Orange;
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            sw.Stop();
                            Interlocked.Increment(ref _failureCount);
                            this.Invoke((MethodInvoker)delegate
                            {
                                listViewItem.SubItems[1].Text = L10n.Get("StatusFailed");
                                listViewItem.SubItems[2].Text = ex.Message;
                                listViewItem.ForeColor = Color.Red;
                            });
                        }
                        finally
                        {
                            Interlocked.Increment(ref _processedCount);
                            this.Invoke((MethodInvoker)delegate
                            {
                                progressBar1.Value = _processedCount;
                                UpdateStatus();
                            });
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                Task.WhenAll(tasks).GetAwaiter().GetResult();
            }
        }

        private void ConvertHeicToJpg(string inputPath, string outputPath)
        {
            using (var fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                // Openize.HEIC을 사용하여 로드
                var image = HeicImage.Load(fs);
                int width = (int)image.Width;
                int height = (int)image.Height;

                // Bgra32 형식으로 픽셀 데이터 추출
                var pixels = image.GetByteArray(Openize.Heic.Decoder.PixelFormat.Bgra32);

                // System.Drawing.Bitmap 생성
                using (var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    BitmapData bmpData = bitmap.LockBits(
                        new System.Drawing.Rectangle(0, 0, width, height),
                        ImageLockMode.WriteOnly,
                        bitmap.PixelFormat);

                    // 픽셀 데이터 복사
                    Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);

                    bitmap.UnlockBits(bmpData);

                    // JPG로 저장 (덮어쓰기)
                    bitmap.Save(outputPath, ImageFormat.Jpeg);
                }
            }
        }

        private bool CheckOverwrite(string fileName)
        {
            // 이미 결정된 상태가 있으면 즉시 반환
            if (_overwriteMode == OverwriteMode.OverwriteAll) return true;
            if (_overwriteMode == OverwriteMode.SkipAll) return false;

            lock (_dialogLock)
            {
                // Lock 획득 후 다시 한 번 체크 (다른 스레드가 먼저 결정했을 수 있음)
                if (_overwriteMode == OverwriteMode.OverwriteAll) return true;
                if (_overwriteMode == OverwriteMode.SkipAll) return false;

                bool result = false;
                this.Invoke((MethodInvoker)delegate
                {
                    using (var dlg = new OverwriteDialog(fileName))
                    {
                        var dr = dlg.ShowDialog(this);
                        result = (dr == DialogResult.Yes);
                        if (dlg.ApplyToAll)
                        {
                            _overwriteMode = result ? OverwriteMode.OverwriteAll : OverwriteMode.SkipAll;
                        }
                    }
                });
                return result;
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
