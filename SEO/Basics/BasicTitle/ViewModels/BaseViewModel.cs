namespace BasicTitle.ViewModels
{
    public abstract class BaseViewModel
    {
        public BaseViewModel(string title)
        {
            this.Title = title;
        }

        public string Title { get; set; }
    }
}
