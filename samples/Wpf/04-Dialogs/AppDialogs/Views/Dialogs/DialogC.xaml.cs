﻿using System.Windows.Controls;
using DryIoc;
using AppDialogs.ViewModels.Dialogs;

namespace AppDialogs.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogC.xaml
    /// </summary>
    public partial class DialogC : UserControl
    {
        public DialogC()
        {
            DataContext = App.Container.Resolve<DialogCViewModel>();
            InitializeComponent();
        }
    }
}
