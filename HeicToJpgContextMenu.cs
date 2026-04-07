using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System.IO;
using System.Reflection;

namespace HeicToJpgShell
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".heic")]
    [Guid("A3D7395B-883F-4D2A-9817-A9684C511B34")]
    public class HeicToJpgContextMenu : SharpContextMenu
    {
        [ThreadStatic]
        private static bool isResolving = false;

        static HeicToJpgContextMenu()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                if (isResolving) return null;

                try
                {
                    isResolving = true;
                    string name = new AssemblyName(args.Name).Name;

                    string folderPath = Path.GetDirectoryName(typeof(HeicToJpgContextMenu).Assembly.Location);
                    string assemblyPath = Path.Combine(folderPath, name + ".dll");

                    if (File.Exists(assemblyPath))
                    {
                        return Assembly.LoadFrom(assemblyPath);
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is BadImageFormatException || ex is Exception)
                {
                    // Intentionally suppressed: assembly resolution failures must not propagate
                    // to the shell host (Explorer). Returning null signals the runtime to continue
                    // with other resolvers.
                    _ = ex;
                }
                finally
                {
                    isResolving = false;
                }
                return null;
            };
        }
        /// <summary>
        /// 메뉴를 표시할지 여부를 결정합니다.
        /// </summary>
        protected override bool CanShowMenu()
        {
            // 선택된 파일 중 .heic 확장자가 하나라도 있으면 true
            return SelectedItemPaths.Any(p => p.EndsWith(".heic", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 컨텍스트 메뉴 아이템을 생성합니다.
        /// </summary>
        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            var itemCount = SelectedItemPaths
                .Count(p => p.EndsWith(".heic", StringComparison.OrdinalIgnoreCase));

            // 메뉴명: L10n 적용
            var item = new ToolStripMenuItem
            {
                Text = L10n.Get("MenuText"),
            };

            item.Click += (sender, args) => ConvertFiles();

            menu.Items.Add(item);

            return menu;
        }

        /// <summary>
        /// 실제 변환 프로세스를 시작합니다.
        /// </summary>
        private void ConvertFiles()
        {
            try
            {
                // ProgressForm을 생성하고 선택된 파일 목록을 전달합니다.
                using (var form = new ProgressForm(SelectedItemPaths))
                {
                    // 쉘 확장 내에서 폼을 띄울 때는 최상위로 띄우거나 
                    // 부모 핸들을 찾아서 연결하는 것이 좋습니다.
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(L10n.Format("ErrorMsg", ex.Message), L10n.Get("ErrorTitle"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
