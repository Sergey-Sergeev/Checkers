namespace Assets
{
    internal interface ITab
    {
        public bool IsVisible { get; }
        public void Show();
        public void Hide();
    }
}