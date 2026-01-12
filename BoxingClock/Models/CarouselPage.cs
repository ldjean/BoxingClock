using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxingClock.Models
{
    public class CarouselPage
    {
        public string Title { get; set; }
        public Type PageType { get; set; }

        public ContentView PageContent
        {
            get
            {
                if (PageType != null)
                {
                    var page = Activator.CreateInstance(PageType) as ContentView;
                    page.BindingContext = App.MainViewModel;
                    return page;
                }
                return null;
            }
        }
    }
}
