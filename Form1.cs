namespace FileCompare
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void PopulateListView(ListView lv, string folderPath)
        {
            lv.BeginUpdate(); lv.Items.Clear();
            try
            {
                string comparePath = "";
                if (lv == lvwLeftDir) comparePath = txtRightDir.Text;
                else if (lv == lvwrightDir) comparePath = txtLeftDir.Text;

                // 폴더(디렉터리) 먼저 추가
                var dirs = Directory.EnumerateDirectories(folderPath).Select(p => new DirectoryInfo(p)).OrderBy(d => d.Name);
                foreach (var d in dirs)
                {
                    var item = new ListViewItem(d.Name);
                    item.SubItems.Add("<DIR>");
                    item.SubItems.Add(d.LastWriteTime.ToString("g"));
                    lv.Items.Add(item);
                }

                // 파일 추가
                var files = Directory.EnumerateFiles(folderPath).Select(p => new FileInfo(p)).OrderBy(f => f.Name);
                foreach (var f in files)
                {
                    var item = new ListViewItem(f.Name);
                    item.SubItems.Add(f.Length.ToString("N0") + " 바이트");
                    item.SubItems.Add(f.LastWriteTime.ToString("g"));

                    // 반대쪽 폴더와 파일을 비교하여 색상 지정
                    if (!string.IsNullOrWhiteSpace(comparePath) && Directory.Exists(comparePath))
                    {
                        string compareFilePath = Path.Combine(comparePath, f.Name);

                        if (File.Exists(compareFilePath))
                        {
                            DateTime compTime = File.GetLastWriteTime(compareFilePath);
                            int timeDiff = f.LastWriteTime.CompareTo(compTime);

                            if (timeDiff == 0) item.ForeColor = Color.Black;
                            else if (timeDiff > 0) item.ForeColor = Color.Red;
                            else item.ForeColor = Color.Gray;
                        }
                        else
                        {
                            item.ForeColor = Color.Purple;
                        }
                    }
                    else
                    {
                        // [수정 완료] 비교할 대상 폴더가 없으면 기본 검정색
                        item.ForeColor = Color.Black;
                    }

                    lv.Items.Add(item);
                }

                for (int i = 0; i < lv.Columns.Count; i++)
                {
                    lv.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show(this, "폴더를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show(this, "입출력 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lv.EndUpdate();
            }
        }

        private void btnLeftDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를 선택하세요.";
                // 현재 텍스트박스에 있는 경로를 초기 선택 폴더로 설정
                if (!string.IsNullOrWhiteSpace(txtLeftDir.Text) && Directory.Exists(txtLeftDir.Text))
                {
                    dlg.SelectedPath = txtLeftDir.Text;
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtLeftDir.Text = dlg.SelectedPath;
                    PopulateListView(lvwLeftDir, dlg.SelectedPath);
                }

                if (Directory.Exists(txtRightDir.Text))
                {
                    PopulateListView(lvwrightDir, txtRightDir.Text);
                }
            }
        }

        private void btnRightDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를 선택하세요.";
                // 현재 텍스트박스에 있는 경로를 초기 선택 폴더로 설정
                if (!string.IsNullOrWhiteSpace(txtRightDir.Text) && Directory.Exists(txtRightDir.Text))
                {
                    dlg.SelectedPath = txtRightDir.Text;
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtRightDir.Text = dlg.SelectedPath;
                    PopulateListView(lvwrightDir, dlg.SelectedPath);
                }

                if (Directory.Exists(txtLeftDir.Text))
                {
                    PopulateListView(lvwLeftDir, txtLeftDir.Text);
                }
            }
        }

        private void btnCopyFromLeft_Click(object sender, EventArgs e)
        {

        }

        private void btnCopyFromRight_Click(object sender, EventArgs e)
        {

        }
    }
}
