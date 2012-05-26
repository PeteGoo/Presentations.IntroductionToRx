using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using AutoCompleteWithRx.Commands;

namespace AutoCompleteWithRx {
    public class AutoCompleteViewModel : INotifyPropertyChanged {

        public AutoCompleteViewModel() {
            var searchTextChanges = (from change in PropertyChanges
                                    where change.EventArgs.PropertyName == "SearchText"
                                    select SearchText)
                                    //.Throttle(TimeSpan.FromSeconds(1))
                                    .Merge(TextBoxEnterCommand.Executed.Select(s => SearchText))
                                    .DistinctUntilChanged();

            searchTextChanges.ObserveOnDispatcher().Subscribe(s => Log("Text Changed: {0}", s));

            var doSearch = Observable.ToAsync<string, SearchResult>(Search);

            var searches = from s in searchTextChanges
                           from result in doSearch(s).TakeUntil(searchTextChanges)
                           select result;

            searches.ObserveOnDispatcher().Subscribe(s => Log("Search Completed for {0}", s.SearchTerm));

            searches.ObserveOnDispatcher().Subscribe(r => {
                SearchResults.Clear();
                r.Results.ToList().ForEach(item => SearchResults.Add(item));
            });
        }

        private string searchText;

        public string SearchText     {
            get { return searchText; }
            set {
                if (searchText == value) {
                    return;
                }
                searchText = value;
                NotifyPropertyChanged("SearchText");
            }
        }

        private readonly ObservableCollection<string> logOutput = new ObservableCollection<string>();

        public ObservableCollection<string> LogOutput {
            get { return logOutput; }
        }

        private readonly ObservableCollection<string> searchResults = new ObservableCollection<string>();

        public ObservableCollection<string> SearchResults {
            get { return searchResults; }
        }

        
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Log(string message, params object[] replacements) {
            LogOutput.Insert(0, string.Format(message, replacements));
        }

        #region Search

        private readonly Random random = new Random(5);

        protected SearchResult Search(string phrase) {
            //Thread.Sleep(200); //Simulate latency
            Thread.Sleep(random.Next(100, 500)); // Simulate load based latency
            return new SearchResult {
                SearchTerm = phrase,
                Results =
                    phrases.Where(item => item.ToUpperInvariant().Contains(phrase.ToUpperInvariant())).ToArray()
            };
        }

        private readonly string[] phrases = new[] {
            "The badger knows something",
            "Your head looks like a pineapple",
            "Crazy like a box of frogs",
            "Can you smell toast?",
            "We're going to need some golf shoes",
            "I think I'm getting the Fear",
            "There's someone at the door",
            "We can't stop here. This is bat country.",
            "It's okay. He's just admiring the shape of your skull.",
            "Let's get down to brass tacks. How much for the ape?",
            "We want... a shrubbery",
            "What is the airspeed velocity of an unladen swallow?",
            "I unplug my nose in your general direction, sons-of-a-windowdresser! ",
            "Nobody expects the Spanish Inquisition",
            "Well, that's no ordinary rabbit",
            "Dale dug a hole. Tell 'em Dale - Dale: I dug a hole",
            "Hows the serenity?",
            "Dad, he reckons powerlines are a reminder of man's ability to generate electricity.",
            "Tell 'em they're dreamin'.",
            "This is going straight to the pool room",
            "We're going to Bonnie Doon!",
            "Steve is also an ideas man. That's why Dad calls him the Ideas Man. He has lots of ideas.",
            "It's a motorcycle helmet with a built-in brake light",
            "Hansel, so hot right now...Hansel.",
            "ORANGE MOCHA FRAPPUCCINO!!!",
            "I'm not an ambi-turner",
            "If you can dodge a wrench, you can dodge a ball.",
            "If you can dodge traffic, you can dodge a ball.",
            "React",
            "Reaction",
            "Reactionary",
            "Reactive",
            "Reactor",
            "Reactivate"
        };

        

        public struct SearchResult {
            public string SearchTerm { get; set; }
            public IEnumerable<string> Results { get; set; }
        }

        #endregion

        #region PropertyChanges

        private IObservable<EventPattern<PropertyChangedEventArgs>> propertyChanges;

        public IObservable<EventPattern<PropertyChangedEventArgs>> PropertyChanges {
            get {
                if(propertyChanges == null) {
                    propertyChanges = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    ev => PropertyChanged += ev,
                    ev => PropertyChanged -= ev);
                }
                return propertyChanges;
            }
        }

        #endregion

        #region Text Box Enter Command

        private RelayCommand textBoxEnterCommand;

        public RelayCommand TextBoxEnterCommand {
            get {
                if (textBoxEnterCommand == null) {
                    textBoxEnterCommand = new RelayCommand();
                    textBoxEnterCommand.Executed.ObserveOnDispatcher().Subscribe(s => Log("Enter Key Pressed"));
                }
                return textBoxEnterCommand;
            }
            set { textBoxEnterCommand = value; }
        }

        #endregion



    }


}