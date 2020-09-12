namespace DataToolChain
{
    public class DropDownDisplay<TValue>
    {
        public string DisplayText { get; set; }
        public TValue Value { get; set; }

        public DropDownDisplay()
        {

        }

        public DropDownDisplay(string displayText, TValue value)
        {
            this.DisplayText = displayText;
            this.Value = value;
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
