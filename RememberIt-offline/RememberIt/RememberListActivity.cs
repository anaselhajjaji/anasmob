
using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace RememberIt
{
    [Activity(Label = "Things to remember")]
    public class RememberListActivity : AppCompatActivity
    {
        RecyclerView recyclerView;
        RecyclerView.LayoutManager layoutManager;
        RememberThingsAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.RememberList);

            // Setup the toolbar
            Android.Support.V7.Widget.Toolbar myToolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.my_toolbar);
            SetSupportActionBar(myToolbar);

            // Initialize the recycler view
            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            layoutManager = new LinearLayoutManager(this);
            recyclerView.SetLayoutManager(layoutManager);

            // Populate the list
            PopulateList();
        }

        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.MainMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_add:
                    CreateNewItem();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        void CreateNewItem()
        {
            View dialogView = LayoutInflater.Inflate(Resource.Layout.ItemCreate, null);

            // Retrieve the components
            EditText name = dialogView.FindViewById<EditText>(Resource.Id.editText1);
            DatePicker datePicker = dialogView.FindViewById<DatePicker>(Resource.Id.datePicker1);
            TimePicker timePicker = dialogView.FindViewById<TimePicker>(Resource.Id.timePicker1);

            // Build the dialog
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetPositiveButton("Create", (sender, e) => CommitCreation(name.Text, datePicker, timePicker));
            builder.SetNegativeButton("Cancel", (sender, e) => { });
            Android.Support.V7.App.AlertDialog dialog = builder.Create();
            dialog.SetView(dialogView);
            dialog.Show();
        }

        void CommitCreation(string name, DatePicker datepicker, TimePicker timepicker)
        {
            RememberThing rememberThing = new RememberThing();
            rememberThing.Name = name;

            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            // Init values
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                hours = timepicker.Hour;
                minutes = timepicker.Minute;
            }
            else
            {
                hours = timepicker.CurrentHour.IntValue();
                minutes = timepicker.CurrentMinute.IntValue();
            }

            rememberThing.Deadline = new DateTime(datepicker.DateTime.Year, datepicker.DateTime.Month,
                                                  datepicker.DateTime.Day, hours, minutes, seconds); 

            adapter.RememberThings.Insert(0, rememberThing);
            adapter.NotifyItemInserted(0);
        }

        /// <summary>
        /// Populates the list.
        /// </summary>
        void PopulateList()
        {
            // Let's create some dummy elements
            RememberThing thing1 = new RememberThing();
            thing1.Name = "Bring the milk";
            thing1.Deadline = DateTime.Now.AddMinutes(10);

            RememberThing thing2 = new RememberThing();
            thing2.Name = "Bring the bread";
            thing2.Deadline = DateTime.Now.AddMinutes(20);

            List<RememberThing> rememberThings = new List<RememberThing>();
            rememberThings.Add(thing1);
            rememberThings.Add(thing2);

            adapter = new RememberThingsAdapter(rememberThings);
            recyclerView.SetAdapter(adapter);
        }

        /// <summary>
        /// The adapter.
        /// </summary>
        public class RememberThingsAdapter : RecyclerView.Adapter
        {
            public List<RememberThing> RememberThings { get; set; }

            public RememberThingsAdapter(List<RememberThing> rememberThings)
            {
                RememberThings = rememberThings;
            }

            /// <summary>
            /// The View Holder
            /// </summary>
            public class RememberThingsViewHolder : RecyclerView.ViewHolder
            {
                TextView name;
                TextView deadline;

                public RememberThingsViewHolder(View view) : base(view)
                {
                    name = view.FindViewById<TextView>(Resource.Id.name);
                    deadline = view.FindViewById<TextView>(Resource.Id.deadline);
                }

                public void BindViewHolder(RememberThing rememberThing)
                {
                    name.Text = rememberThing.Name;
                    deadline.Text = rememberThing.Deadline.ToString();
                }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.RememberThing, parent, false);
                return new RememberThingsViewHolder(view);
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                RememberThing rememberThing = RememberThings[position];
                (holder as RememberThingsViewHolder).BindViewHolder(rememberThing);
            }

            public override int ItemCount
            {
                get
                {
                    return RememberThings.Count;
                }
            }
        }
    }
}
