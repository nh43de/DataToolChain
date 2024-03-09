using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace DataToolChain.Ui.Extensions
{

    public static class ExceptionExtensions
    {
        public static Exception Display(this Exception ex, string msg = null,
            MessageBoxImage img = MessageBoxImage.Error)
        {
            MessageBox.Show(msg ?? ex.Message, "", MessageBoxButton.OK, img);
            return ex;
        }

        public static Exception DisplayInners(this Exception ex, string caption = "", string additionalText = "",
            MessageBoxImage img = MessageBoxImage.Error)
        {
            var exs = new List<Exception>
            {
                ex
            };

            var iex = ex.InnerException;
            while (iex != null)
            {
                exs.Add(iex);
                iex = iex.InnerException;
            }

            MessageBox.Show(additionalText + "\r\n\r\nException info: " + string.Join(" ", exs.Select(e => e.Message)),
                caption, MessageBoxButton.OK, img);
            return ex;
        }
    }


}