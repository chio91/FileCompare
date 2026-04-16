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

        private void CopySelectedFiles(ListView sourceView, string sourceDir, string destDir)
        {
            // 1. 경로 유효성 검사
            if (string.IsNullOrWhiteSpace(sourceDir) || string.IsNullOrWhiteSpace(destDir) ||
                !Directory.Exists(sourceDir) || !Directory.Exists(destDir))
            {
                MessageBox.Show(this, "양쪽 폴더 경로가 모두 올바르게 지정되어야 복사할 수 있습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 2. 선택된 항목 확인
            if (sourceView.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "복사할 파일을 선택해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int copyCount = 0;

            foreach (ListViewItem item in sourceView.SelectedItems)
            {
                // 폴더인 경우 복사에서 제외 (이름이 <DIR>인 하위 항목을 가짐)
                if (item.SubItems.Count > 1 && item.SubItems[1].Text == "<DIR>")
                {
                    continue;
                }

                string fileName = item.Text;
                string sourceFile = Path.Combine(sourceDir, fileName);
                string destFile = Path.Combine(destDir, fileName);

                // 3. [수정된 부분] 오래된 파일(회색) 덮어쓰기 경고 시 상세 정보 표시
                // 3. 오래된 파일(회색) 덮어쓰기 경고 시 상세 정보 표시
                if (item.ForeColor == Color.Gray)
                {
                    // 파일의 실제 수정 시간 가져오기
                    DateTime sourceTime = File.GetLastWriteTime(sourceFile);
                    DateTime destTime = File.GetLastWriteTime(destFile);

                    // 첨부한 이미지의 형식에 맞춰 메시지 구성
                    string warningMessage = "대상에 동일한 이름의 파일이 이미 있습니다.\n" +
                                            "대상 파일이 더 신규 파일입니다. 덮어쓰시겠습니까?\n\n" +
                                            $"원본: {sourceFile}\n" +
                                            $"수정된 날짜: {sourceTime:yyyy-MM-dd HH:mm:ss}\n" +
                                            $"대상: {destFile}\n" +
                                            $"수정된 날짜: {destTime:yyyy-MM-dd HH:mm:ss}";

                    // 아이콘을 Question(물음표)으로 변경
                    DialogResult result = MessageBox.Show(this, warningMessage, "덮어쓰기 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                    {
                        continue; // '아니오'를 누르면 이 파일은 건너뛰고 다음 파일로 진행
                    }
                }

                // 4. 파일 복사 수행
                try
                {
                    File.Copy(sourceFile, destFile, true); // true: 기존 파일 덮어쓰기 허용
                    copyCount++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"'{fileName}' 복사 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // 5. [수정된 부분] 복사 완료 메시지 제거, 양쪽 화면 갱신만 수행
            if (copyCount > 0)
            {
                PopulateListView(lvwLeftDir, txtLeftDir.Text);
                PopulateListView(lvwrightDir, txtRightDir.Text);
            }
        }

        // 기존 버튼 클릭 이벤트에 메서드 연결
        private void btnCopyFromLeft_Click(object sender, EventArgs e)
        {
            // 왼쪽(lvwLeftDir)에서 선택한 파일을 왼쪽 경로(txtLeftDir)에서 오른쪽 경로(txtRightDir)로 복사
            CopySelectedFiles(lvwLeftDir, txtLeftDir.Text, txtRightDir.Text);
        }

        private void btnCopyFromRight_Click(object sender, EventArgs e)
        {
            // 오른쪽(lvwrightDir)에서 선택한 파일을 오른쪽 경로(txtRightDir)에서 왼쪽 경로(txtLeftDir)로 복사
            CopySelectedFiles(lvwrightDir, txtRightDir.Text, txtLeftDir.Text);
        }
    }
}
