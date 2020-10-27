// This defines a structure that stores T in a queue
// The queue uses a linked list data structure
public class LinkedQueue<T>
{
    // Since we're a linked list queue, we pop at tail and push at head
    QueueNode tail;
    QueueNode head;

    public int Size { get; set; }

    // Push the element of type T into the queue
    public void Push (T element)
    {
        QueueNode elem = new QueueNode(element);

        if (head == null || tail == null)
        {
            head = elem;
            tail = elem;
            Size = 1;
        }
        else
        {
            elem.Next = head;
            head.Prev = elem;
            head = elem;
            Size += 1;
        }
    }

    // Pop the queue, removing and returning the next element
    public T Pop ()
    {
        if (isEmpty ())
            throw new QueueEmpty("Error, pop when empty.");

        QueueNode val = tail;

        if (tail.Prev != null)
        {
            tail.Prev = null; // For garbage collector
            tail = val.Prev;
            tail.Next = null;
            Size -= 1;
        }
        else
        {
            // Removed last element
            tail = null;
            head = null;
            Size = 0;
        }

        return val.Value;
    }

    // Returns true if the list is empty
    bool isEmpty ()
    {
        return Size <= 0 || tail == null || head == null;
    }

    class QueueNode
    {
        public T Value { get; private set; }
        public QueueNode Next { get; set; }
        public QueueNode Prev { get; set; }

        public QueueNode(T value)
        {
            this.Value = value;
        }

        public QueueNode(T value, QueueNode next, QueueNode prev)
        {
            this.Value = value;
            this.Next = next;
            this.Prev = prev;
        }
    }
}
