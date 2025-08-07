using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace ExchangeBooks.Behaviors
{
    public class PickerBehavior: Behavior<Picker>
    {
        public PickerBehavior()
        {
        }
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(PickerBehavior), null);

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public Picker Bindable { get; private set; }

        protected override void OnAttachedTo(Picker bindable)
        {
            base.OnAttachedTo(bindable);
            Bindable = bindable;
            Bindable.BindingContextChanged += OnBindingContextChanged;
            Bindable.SelectedIndexChanged += OnSwitchToggled;
        }

        protected override void OnDetachingFrom(Picker bindable)
        {
            base.OnDetachingFrom(bindable);
            Bindable.BindingContextChanged -= OnBindingContextChanged;
            Bindable.SelectedIndexChanged -= OnSwitchToggled;
            Bindable = null;
        }
        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            OnBindingContextChanged();
            BindingContext = Bindable.BindingContext;
        }

        private void OnSwitchToggled(object sender, EventArgs e)
        {
            var picker = ((Picker)sender);
            if (picker == null || picker.SelectedIndex < 0) return;

            Command?.Execute(picker.SelectedItem.ToString());
        }
    }
}
