namespace Phosphorescence.Narration
{
    public interface ICanProcessLine
    {
        public void Initialize(OnLineReadEvent line);
        public void Process();
    }

    public interface ICanProcessLines
    {
        public void Initialize(OnLinesReadEvent lines);
        public void Process();
    }
}