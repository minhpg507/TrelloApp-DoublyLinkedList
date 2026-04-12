using System;

namespace TrelloAppMinh
{
    // Đây là "linh hồn" chứa dữ liệu của 1 thẻ
    public class CardNode
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }
        public string Status { get; set; } // "TODO", "DOING", hoặc "DONE"

        // Dòng này để chứa đường dẫn file
        public string AttachedFilePath { get; set; }
        // Hai cánh tay để nắm lấy thẻ phía trước và phía sau
        public CardNode Prev { get; set; }
        public CardNode Next { get; set; }

        // Hàm khởi tạo
        public CardNode(string title, string message, string date, string status, string attachedFilePath = "")
        {
            Title = title;
            Message = message;
            Date = date;
            Status = status;
            AttachedFilePath = attachedFilePath; // Cập nhật đường dẫn
            Prev = null;
            Next = null;
        }
    }

    // Đây là Cấu trúc dữ liệu quản lý 1 cột
    public class CardLinkedList
    {
        public CardNode Head; // Thẻ đầu tiên trong cột
        public CardNode Tail; // Thẻ cuối cùng trong cột

        public CardLinkedList()
        {
            Head = null;
            Tail = null;
        }

        // 1. Thêm một thẻ mới vào cuối danh sách (Khi bấm nút New)
        public void AddLast(CardNode newNode)
        {
            if (Head == null)
            {
                Head = Tail = newNode;
            }
            else
            {
                Tail.Next = newNode;
                newNode.Prev = Tail;
                Tail = newNode;
            }
        }

        // 2. Rút một thẻ ra khỏi danh sách (Khi bạn nhấc thẻ kéo đi cột khác)
        public void Remove(CardNode target)
        {
            if (Head == null || target == null) return;

            CardNode current = Head;
            while (current != null)
            {
                if (current == target)
                {
                    // 1. Cắt dây với người đứng trước
                    if (current.Prev != null)
                        current.Prev.Next = current.Next;
                    else
                        Head = current.Next; // Nếu nó là thẻ đầu tiên

                    // 2. Cắt dây với người đứng sau
                    if (current.Next != null)
                        current.Next.Prev = current.Prev;
                    else
                        Tail = current.Prev; // Nếu nó là thẻ cuối cùng

                    // Xóa sạch dấu vết
                    current.Prev = null;
                    current.Next = null;
                    return;
                }
                current = current.Next;
            }
        }
        // 3. Xóa sạch toàn bộ cột
        public void Clear()
        {
            Head = null;
            Tail = null;
        }

        // 4. Giải thuật Sắp xếp chọn (Selection Sort) theo chiều A-Z của Tiêu đề
        public void SelectionSort()
        {
            if (Head == null || Head.Next == null) return;

            for (CardNode i = Head; i != null; i = i.Next)
            {
                CardNode minNode = i;

                for (CardNode j = i.Next; j != null; j = j.Next)
                {
                    if (string.Compare(j.Title, minNode.Title, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        minNode = j;
                    }
                }

                if (minNode != i)
                {
                    string tempTitle = i.Title;
                    i.Title = minNode.Title;
                    minNode.Title = tempTitle;

                    string tempMsg = i.Message;
                    i.Message = minNode.Message;
                    minNode.Message = tempMsg;

                    string tempDate = i.Date;
                    i.Date = minNode.Date;
                    minNode.Date = tempDate;
                }
            }
        }
        // 5. Giải thuật Sắp xếp nổi bọt (Bubble Sort) theo chiều Z-A
        public void BubbleSortZA()
        {
            if (Head == null || Head.Next == null) return;
            bool swapped;
            do
            {
                swapped = false;
                CardNode current = Head;
                while (current.Next != null)
                {
                    // So sánh: Nếu chữ cái đứng trước lại nhỏ hơn chữ cái đứng sau (A đứng trước Z) thì tráo đổi
                    if (string.Compare(current.Title, current.Next.Title, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        // Hoán đổi Title
                        string tempTitle = current.Title;
                        current.Title = current.Next.Title;
                        current.Next.Title = tempTitle;

                        // Hoán đổi Message
                        string tempMsg = current.Message;
                        current.Message = current.Next.Message;
                        current.Next.Message = tempMsg;

                        // Hoán đổi Date
                        string tempDate = current.Date;
                        current.Date = current.Next.Date;
                        current.Next.Date = tempDate;

                        // Hoán đổi FilePath (Quan trọng để không rớt file)
                        string tempFile = current.AttachedFilePath;
                        current.AttachedFilePath = current.Next.AttachedFilePath;
                        current.Next.AttachedFilePath = tempFile;

                        swapped = true;
                    }
                    current = current.Next;
                }
            } while (swapped);
        }
    }
}