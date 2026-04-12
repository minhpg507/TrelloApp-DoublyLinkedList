using System;
using System.Drawing;
using System.Windows.Forms;

namespace TrelloAppMinh
{
    public partial class NoteCard : UserControl
    {

        // --- 1. TẠO "CỬA GIAO TIẾP" ĐỂ FORM1 TRUYỀN DỮ LIỆU ---
        public string Title
        {
            get { return txtTitle.Text; }
            set { txtTitle.Text = value; }
        }

        public string Message
        {
            get { return txtMessage.Text; }
            set { txtMessage.Text = value; }
        }

        public string Date
        {
            get { return lblDate.Text; }
            set { lblDate.Text = value; }
        }

        // Đây chính là con chip kết nối giao diện với phần dữ liệu chạy ngầm
        public CardNode NodeData { get; set; }



        // --- 2. HÀM KHỞI TẠO (Đã kết hợp code cũ của bạn) ---
        public NoteCard(CardNode node)
        {
            InitializeComponent();
            // --- THÊM 3 DÒNG NÀY ĐỂ ÉP KẾT NỐI NÚT BẤM ---
            btnRead.Click += btnRead_Click;
            btnAddFile.Click += btnAddFile_Click;
            btnOpen.Click += btnOpen_Click;
            // ---------------------------------------------

            NodeData = node;
            Title = node.Title;
            Message = node.Message;
            Date = node.Date;

            NodeData = node;

            // Đổ dữ liệu từ Node vào giao diện ngay khi thẻ được sinh ra
            Title = node.Title;
            Message = node.Message;
            Date = node.Date;

            // Mở khóa cho thẻ và các chữ bên trong có thể nhận diện Kéo-Thả
            this.AllowDrop = true;
            txtTitle.AllowDrop = true;
            txtMessage.AllowDrop = true;

            // Dùng code tự động nối dây điện Kéo-Thả (Chuẩn code cũ của bạn)
            this.DragEnter += The_BaoCoVatBayQua;
            txtTitle.DragEnter += The_BaoCoVatBayQua;
            txtMessage.DragEnter += The_BaoCoVatBayQua;

            this.DragDrop += The_BaoNhaChuot;
            txtTitle.DragDrop += The_BaoNhaChuot;
            txtMessage.DragDrop += The_BaoNhaChuot;
        }

        // --- 3. XỬ LÝ KÉO THẢ TẠI CHỖ (Tính năng xịn của Hello Universe) ---
        private void The_BaoCoVatBayQua(object sender, DragEventArgs e)
        {
            // Kiểm tra xem đồ đang bị kéo lơ lửng có đúng là NoteCard không
            if (e.Data.GetDataPresent(typeof(NoteCard)))
            {
                e.Effect = DragDropEffects.Move; // Đổi icon chuột
            }
        }

        private void The_BaoNhaChuot(object sender, DragEventArgs e)
        {
            NoteCard draggedCard = (NoteCard)e.Data.GetData(typeof(NoteCard));

            // Tìm xem thẻ đang bị đè lên nằm ở cột nào
            FlowLayoutPanel targetColumn = (FlowLayoutPanel)this.Parent;

            if (targetColumn != null && draggedCard != null && draggedCard != this)
            {
                // 1. Nếu thẻ bay từ cột khác sang, Form1 đã xử lý add nó vào, ở đây ta chỉ quan tâm XẾP CHỖ.
                if (draggedCard.Parent != targetColumn)
                {
                    targetColumn.Controls.Add(draggedCard);
                }

                // 2. XẾP CHỖ: Đẩy thẻ lơ lửng vào đúng vị trí của thẻ đang bị đè (Đảo vị trí 2 thẻ)
                int targetIndex = targetColumn.Controls.GetChildIndex(this);
                targetColumn.Controls.SetChildIndex(draggedCard, targetIndex);

                Form1 mainForm = this.FindForm() as Form1;
                if (mainForm != null)
                {
                    mainForm.SyncDataFromUI();
                    mainForm.btnSave_Click(null, null); // Lưu ngầm không hiện thông báo
                }
            }
        }

        // --- 4. CHỨC NĂNG CÁC NÚT BẤM VÀ TƯƠNG TÁC ---

        // Khi người dùng bấm giữ chuột trái vào Tiêu đề để nhấc thẻ đi
        private void txtTitle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.DoDragDrop(this, DragDropEffects.Move);
            }
        }

        // Nút Read
        private void btnRead_Click(object sender, EventArgs e)
        {
            // Lệnh này phát ra tín hiệu "Cái thẻ vừa bị click" 
            // Ngay lập tức Form1 sẽ bắt được tín hiệu và đưa chữ lên TextBox để bạn Update
            this.OnClick(e);
        }

        // Nút Add File
        private void btnAddFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Chọn file để đính kèm vào thẻ";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // Lưu đường dẫn thẳng vào Linh hồn (NodeData)
                if (NodeData != null)
                {
                    NodeData.AttachedFilePath = ofd.FileName;
                }

                string fileName = System.IO.Path.GetFileName(ofd.FileName);
                Message += "\r\n\r\n[File đính kèm: " + fileName + "]";
                if (NodeData != null) NodeData.Message = Message;

                MessageBox.Show("Đã đính kèm file thành công! Bạn hãy bấm Save All để lưu vĩnh viễn.", "Thông báo");
            }
        }

        // Nút Open File
        private void btnOpen_Click(object sender, EventArgs e)
        {
            // Lấy đường dẫn từ Linh hồn (NodeData) ra để mở
            if (NodeData == null || string.IsNullOrWhiteSpace(NodeData.AttachedFilePath))
            {
                MessageBox.Show("Thẻ này chưa có file nào được đính kèm!", "Thông báo");
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = NodeData.AttachedFilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở file. Có thể file đã bị xóa hoặc đổi tên. Lỗi: " + ex.Message, "Lỗi");
            }
        }
    }
}