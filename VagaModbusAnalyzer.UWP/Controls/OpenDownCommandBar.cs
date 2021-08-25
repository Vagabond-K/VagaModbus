using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Controls
{
    public class OpenDownCommandBar : CommandBar
    {
        public OpenDownCommandBar()
        {
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var layoutRoot = GetTemplateChild("LayoutRoot") as Grid;
            if (layoutRoot != null)
            {
                VisualStateManager.SetCustomVisualStateManager(layoutRoot, new OpenDownCommandBarVisualStateManager());
            }
        }

        public class OpenDownCommandBarVisualStateManager : VisualStateManager
        {
            protected override bool GoToStateCore(Control control, FrameworkElement templateRoot, string stateName, VisualStateGroup group, VisualState state, bool useTransitions)
            {
                if (!string.IsNullOrWhiteSpace(stateName) && stateName.EndsWith("OpenUp"))
                {
                    stateName = stateName.Substring(0, stateName.Length - 6) + "OpenDown";
                }
                return base.GoToStateCore(control, templateRoot, stateName, group, state, useTransitions);
            }
        }
    }
}
