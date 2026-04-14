using System;
using System.Drawing;
using System.Windows.Forms;

namespace TrelloAppMinh
{
    public partial class Form1 : Form
    {
        // Hàm giúp lấy đúng danh sách liên kết dựa trên cái khung đang chứa thẻ
        private CardLinkedList GetListFromPanel(FlowLayoutPanel panel)
        {
            if (panel == flowToDo) return listToDo;
            if (panel == flowDoing) return listDoing;
            if (panel == flowDone) return listDone;
            return null;
        }

        private NoteCard currentSelectedCard = null;

        // KHAI BÁO 3 BỘ NÃO (LINKED LIST) QUẢN LÝ DỮ LIỆU 
        CardLinkedList listToDo = new CardLinkedList();
        CardLinkedList listDoing = new CardLinkedList();
        CardLinkedList listDone = new CardLinkedList();

        public Form1()
        {
            InitializeComponent();

            // Gắn mã cho 3 cột để dễ kéo thả
            flowToDo.Tag = "TODO";
            flowDoing.Tag = "DOING";
            flowDone.Tag = "DONE";

            // Gọi code tự động làm đẹp giao diện
            ApplyTrelloStyle();
            // Tự động tải lại thẻ từ lần dùng trước
            LoadData();
        }

        // I. CHỨC NĂNG NEW
        private void btnNew_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Vui lòng nhập Tiêu đề công việc!");
                return;
            }

            string currentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            CardNode newNode = new CardNode(txtTitle.Text, txtMessage.Text, currentDate, "TODO");
            listToDo.AddLast(newNode);

            NoteCard uiCard = new NoteCard(newNode);

            uiCard.Click += Card_OnReadClick;
            uiCard.MouseDown += Card_MouseDown;
            foreach (Control c in uiCard.Controls)
            {
                c.Click += (s, args) => Card_OnReadClick(uiCard, args);

                if (!(c is Button))
                {
                    c.MouseDown += (s, args) => Card_MouseDown(uiCard, (MouseEventArgs)args);
                }
            }

            uiCard.BackColor = Color.White;

            // Đưa thẻ UI vào cột CẦN LÀM trên màn hình
            flowToDo.Controls.Add(uiCard);

            // Xóa trắng form nhập
            ResetInputs();

            // Tự động lưu
            btnSave_Click(null, null);
        }

        // Hàm phụ trợ để xóa trắng form
        private void ResetInputs()
        {
            txtTitle.Text = "";
            txtMessage.Text = "";
            currentSelectedCard = null;
            // Xóa màu highlight của tất cả các thẻ
            foreach (NoteCard card in flowToDo.Controls) card.BackColor = Color.White;
            foreach (NoteCard card in flowDoing.Controls) card.BackColor = Color.White;
            foreach (NoteCard card in flowDone.Controls) card.BackColor = Color.White;
        }

        // II. CHỨC NĂNG SAVE
        public void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter("data.txt"))
                {
                    SaveListToFile(listToDo, "TODO", sw);
                    SaveListToFile(listDoing, "DOING", sw);
                    SaveListToFile(listDone, "DONE", sw);
                }
                // Tắt thông báo để không bị làm phiền khi code tự động lưu
                if (sender != null) MessageBox.Show("Đã lưu dữ liệu từ Linked List thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        // Hàm phụ để duyệt danh sách liên kết và ghi file
        private void SaveListToFile(CardLinkedList list, string status, System.IO.StreamWriter sw)
        {
            CardNode current = list.Head;
            int safeGuard = 0;
            while (current != null && safeGuard < 500)
            {
                string safeMsg = current.Message.Replace(Environment.NewLine, " ");
                sw.WriteLine($"{status} | {current.Title} | {current.Date} | {safeMsg} | {current.AttachedFilePath}");
                current = current.Next;
                safeGuard++;
            }
        }

        // III. CHỨC NĂNG READ 
        private void Card_OnReadClick(object sender, EventArgs e)
        {
            NoteCard clickedCard = sender as NoteCard;
            if (clickedCard != null)
            {
                ResetInputs();

                currentSelectedCard = clickedCard;

                txtTitle.Text = clickedCard.NodeData.Title;
                txtMessage.Text = clickedCard.NodeData.Message;

                clickedCard.BackColor = Color.LightBlue;
            }
        }

        // IV. CHỨC NĂNG UPDATE
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (currentSelectedCard != null)
            {
                currentSelectedCard.NodeData.Title = txtTitle.Text;
                currentSelectedCard.NodeData.Message = txtMessage.Text;

                currentSelectedCard.Controls["txtTitle"].Text = txtTitle.Text;
                currentSelectedCard.Controls["txtMessage"].Text = txtMessage.Text;

                btnSave_Click(null, null);

                MessageBox.Show("Đã cập nhật thẻ thành công!");
                ResetInputs();
            }
            else
            {
                MessageBox.Show("Vui lòng click chọn một thẻ trước khi Update!");
            }
        }

        // V. CHỨC NĂNG DELETE
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (currentSelectedCard == null)
            {
                MessageBox.Show("Vui lòng click chọn thẻ bạn muốn xóa trước!", "Thông báo");
                return;
            }

            DialogResult dialogResult = MessageBox.Show("Bạn có chắc chắn muốn xóa công việc này vĩnh viễn không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dialogResult == DialogResult.Yes)
            {
                FlowLayoutPanel parentPanel = (FlowLayoutPanel)currentSelectedCard.Parent;
                CardLinkedList parentList = GetListFromPanel(parentPanel);

                if (parentList != null)
                {
                    parentList.Remove(currentSelectedCard.NodeData);
                }

                parentPanel.Controls.Remove(currentSelectedCard);
                ResetInputs();
                btnSave_Click(null, null);

                MessageBox.Show("Đã xóa và cập nhật dữ liệu thành công!");
            }
        }

        // VI. HỆ THỐNG KÉO THẢ (DRAG & DROP)
        private void Card_MouseDown(object sender, MouseEventArgs e)
        {
            NoteCard cardToDrag = sender as NoteCard;
            if (cardToDrag != null && e.Button == MouseButtons.Left)
            {
                cardToDrag.DoDragDrop(cardToDrag, DragDropEffects.Move);
            }
        }

        private void Column_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(NoteCard))) e.Effect = DragDropEffects.Move;
        }

        private void Column_DragDrop(object sender, DragEventArgs e)
        {
            NoteCard draggedCard = (NoteCard)e.Data.GetData(typeof(NoteCard));
            FlowLayoutPanel targetPanel = (FlowLayoutPanel)sender;
            FlowLayoutPanel sourcePanel = (FlowLayoutPanel)draggedCard.Parent;

            if (draggedCard != null && targetPanel != null && targetPanel != sourcePanel)
            {
                CardLinkedList sourceList = GetListFromPanel(sourcePanel);
                CardLinkedList targetList = GetListFromPanel(targetPanel);

                if (sourceList != null && targetList != null)
                {
                    sourceList.Remove(draggedCard.NodeData);
                    targetList.AddLast(draggedCard.NodeData);

                    draggedCard.NodeData.Status = targetPanel.Tag.ToString();
                }

                targetPanel.Controls.Add(draggedCard);
                btnSave_Click(null, null);
            }
        }

        // VII. CHỨC NĂNG EXPORT FILE
        private void btnExport_Click(object sender, EventArgs e)
        {
            if (currentSelectedCard == null)
            {
                MessageBox.Show("Vui lòng click chọn thẻ bạn muốn xuất file trước!", "Thông báo");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Lưu thẻ này thành file";
            sfd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            sfd.FileName = currentSelectedCard.NodeData.Title + ".txt";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName))
                    {
                        sw.WriteLine("=========== THÔNG TIN CÔNG VIỆC ===========");
                        sw.WriteLine("TIÊU ĐỀ: " + currentSelectedCard.NodeData.Title);
                        sw.WriteLine("-------------------------------------------");
                        sw.WriteLine("NỘI DUNG:");
                        sw.WriteLine(currentSelectedCard.NodeData.Message);
                        sw.WriteLine("===========================================");
                    }
                    MessageBox.Show("Đã xuất thẻ ra file thành công!", "Thông báo");
                }
                catch (Exception ex) { MessageBox.Show("Có lỗi khi lưu file: " + ex.Message, "Lỗi"); }
            }
        }

        // VIII. CÁC NÚT SẮP XẾP VÀ LÀM ĐẸP
        private void ApplyTrelloStyle()
        {
            this.BackColor = Color.AliceBlue;
            Color columnColor = Color.FromArgb(235, 236, 240);

            flowToDo.BackColor = columnColor;
            flowDoing.BackColor = columnColor;
            flowDone.BackColor = columnColor;

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = Color.FromArgb(223, 225, 230);
                    btn.Cursor = Cursors.Hand;
                }
            }

            if (this.Controls.ContainsKey("btnNew"))
            {
                Button newBtn = (Button)this.Controls["btnNew"];
                newBtn.BackColor = Color.DodgerBlue;
                newBtn.ForeColor = Color.White;
                newBtn.Font = new Font(newBtn.Font, FontStyle.Bold);
            }
        }

        private void LoadData()
        {
            string filePath = "data.txt";
            if (!System.IO.File.Exists(filePath)) return;

            try
            {
                flowToDo.Controls.Clear();
                flowDoing.Controls.Clear();
                flowDone.Controls.Clear();

                listToDo.Clear();
                listDoing.Clear();
                listDone.Clear();

                string[] lines = System.IO.File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split(new string[] { " | " }, StringSplitOptions.None);

                    if (parts.Length >= 4)
                    {
                        string status = parts[0].Trim().ToUpper();
                        string title = parts[1];
                        string date = parts[2];
                        string message = parts[3];

                        string attachedPath = "";
                        if (parts.Length >= 5)
                        {
                            attachedPath = parts[4];
                        }

                        // Nhồi attachedPath vào hàm khởi tạo
                        CardNode loadedNode = new CardNode(title, message, date, status, attachedPath);
                        NoteCard card = new NoteCard(loadedNode);
                        card.Click += Card_OnReadClick;
                        card.MouseDown += Card_MouseDown;
                        foreach (Control c in card.Controls)
                        {
                            c.Click += (s, args) => Card_OnReadClick(card, args);

                            if (!(c is Button))
                            {
                                c.MouseDown += (s, args) => Card_MouseDown(card, (MouseEventArgs)args);
                            }
                        }

                        card.BackColor = Color.White;

                        if (status == "TODO")
                        {
                            listToDo.AddLast(loadedNode);
                            flowToDo.Controls.Add(card);
                        }
                        else if (status == "DOING")
                        {
                            listDoing.AddLast(loadedNode);
                            flowDoing.Controls.Add(card);
                        }
                        else if (status == "DONE")
                        {
                            listDone.AddLast(loadedNode);
                            flowDone.Controls.Add(card);
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải file: " + ex.Message); }
        }

        private void ReloadPanel(FlowLayoutPanel panel, CardLinkedList list)
        {
            panel.Controls.Clear();
            CardNode current = list.Head;
            while (current != null)
            {
                NoteCard card = new NoteCard(current);
                card.Click += Card_OnReadClick;
                card.MouseDown += Card_MouseDown;
                foreach (Control c in card.Controls)
                {
                    c.Click += (s, args) => Card_OnReadClick(card, args);

                    if (!(c is Button))
                    {
                        c.MouseDown += (s, args) => Card_MouseDown(card, (MouseEventArgs)args);
                    }
                }
                card.BackColor = Color.White;
                panel.Controls.Add(card);
                current = current.Next;
            }
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            // 1. ÉP BUỘC ĐỒNG BỘ: Dọn sạch lỗi dây xích trước khi chạy thuật toán
            SyncDataFromUI();

            // 2. Chạy thuật toán Selection Sort cho cả 3 bộ não cùng lúc
            listToDo.SelectionSort();
            listDoing.SelectionSort();
            listDone.SelectionSort();

            // 3. Vẽ lại toàn bộ màn hình theo thứ tự mới
            ReloadPanel(flowToDo, listToDo);
            ReloadPanel(flowDoing, listDoing);
            ReloadPanel(flowDone, listDone);

            // 4. Tự động lưu kết quả mới mượt mà vào ổ cứng
            btnSave_Click(null, null);

            MessageBox.Show("Đã sắp xếp toàn bộ bảng Trello theo thứ tự A-Z!", "Hoàn tất");
        }

        public void SyncDataFromUI()
        {
            listToDo.Clear();
            listDoing.Clear();
            listDone.Clear();

            foreach (Control c in flowToDo.Controls)
            {
                if (c is NoteCard card)
                {
                    card.NodeData.Status = "TODO";
                    card.NodeData.Prev = null; 
                    card.NodeData.Next = null;
                    listToDo.AddLast(card.NodeData);
                }
            }

            foreach (Control c in flowDoing.Controls)
            {
                if (c is NoteCard card)
                {
                    card.NodeData.Status = "DOING";
                    card.NodeData.Prev = null;
                    card.NodeData.Next = null;
                    listDoing.AddLast(card.NodeData);
                }
            }

            foreach (Control c in flowDone.Controls)
            {
                if (c is NoteCard card)
                {
                    card.NodeData.Status = "DONE";
                    card.NodeData.Prev = null;
                    card.NodeData.Next = null;
                    listDone.AddLast(card.NodeData);
                }
            }
        }

        private void btnSortZA_Click(object sender, EventArgs e)
        {
            // 1. Đồng bộ dữ liệu
            SyncDataFromUI();

            // 2. Gọi thuật toán MỚI (Bubble Sort Z-A)
            listToDo.BubbleSortZA();
            listDoing.BubbleSortZA();
            listDone.BubbleSortZA();

            // 3. Vẽ lại màn hình
            ReloadPanel(flowToDo, listToDo);
            ReloadPanel(flowDoing, listDoing);
            ReloadPanel(flowDone, listDone);

            // 4. Lưu ổ cứng
            btnSave_Click(null, null);

            MessageBox.Show("Đã sắp xếp toàn bộ bảng Trello theo thứ tự Z-A", "Hoàn tất");
        }
    }
}
