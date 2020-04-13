using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Globalization;
using Newtonsoft.Json;
using Windows.UI;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace kalkulatorWalut
{
    /// <summary>
    /// Kujawski Szymon 91457
    /// </summary>
    public sealed partial class MainPage : Page
    {
          string daneNBP = "http://www.nbp.pl/kursy/xml/LastA.xml"; 
        List<PozycjaTabeliA> kursyAktualne = new List<PozycjaTabeliA>();
        public MainPage()
        {
            this.InitializeComponent();
        }

        private  void Grid_Loaded(object sender, RoutedEventArgs e)
        {
             InitComp();




        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private async void InitComp()
        {
            var serwerNBP = new HttpClient();
            string dane = "";
            try
            {
                dane = await serwerNBP.GetStringAsync(new Uri(daneNBP));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            if (dane != "" && dane!=null)
            {
                kursyAktualne.Clear();
                
                XDocument daneKursowe = XDocument.Parse(dane);
              
               kursyAktualne = (from item in daneKursowe.Descendants("pozycja")
                                 select new PozycjaTabeliA()
                                 {
                                     przelicznik = (item.Element("przelicznik").Value),
                                     kod_waluty = (item.Element("kod_waluty").Value),
                                     kurs_sredni = (item.Element("kurs_sredni").Value)
                                 }
                                 ).ToList();
                kursyAktualne.Insert(0, (new PozycjaTabeliA() { kurs_sredni = "1,0000", kod_waluty = "PLN", przelicznik = "1" }));

                lbxZWaluty.ItemsSource = kursyAktualne;
                lbxNaWalute.ItemsSource = kursyAktualne;

             
                data.Text = DateTime.Now.ToString();
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["data"] = data.Text;
                data.Foreground = new SolidColorBrush(Colors.Green);



            }
            else 
            {
                StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.GetFileAsync("LastA.xml");
                var text = await FileIO.ReadTextAsync(file);
                kursyAktualne= JsonConvert.DeserializeObject<List<PozycjaTabeliA>>(text);
                lbxZWaluty.ItemsSource = kursyAktualne;
                lbxNaWalute.ItemsSource = kursyAktualne;

                data.Text = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["data"];
           
                data.Foreground = new SolidColorBrush(Colors.Red);



            }
            if (data.Text.Equals("") && ApplicationData.Current.LocalSettings.Values.ContainsKey("data"))
            {
                data.Text = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["data"];
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("lbxNaWalute"))
            {
                lbxNaWalute.SelectedIndex = (int)Windows.Storage.ApplicationData.Current.LocalSettings.Values["lbxNaWalute"];
            }
            else
            {
                lbxNaWalute.SelectedIndex = 0;
               
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("lbxZWaluty"))
            {
                lbxZWaluty.SelectedIndex = (int)Windows.Storage.ApplicationData.Current.LocalSettings.Values["lbxZWaluty"];
            }
            else
            {
                lbxZWaluty.SelectedIndex = 0;
            }
            save();
        }

        private void txtKwota_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                if (!textBox.Text.Equals(""))
                {
                    var wybranaPozycjaWalutowa = lbxZWaluty.SelectedIndex;
                    PozycjaTabeliA pozycja = kursyAktualne[wybranaPozycjaWalutowa];
                    var x = pozycja.kurs_sredni.Replace(",", ".");
                    var kwota = Convert.ToDouble(textBox.Text);

                    double kursSr = Convert.ToDouble(x);

                    double kwotaPLN = kwota * kursSr;
                    var poz = lbxNaWalute.SelectedIndex;
                    PozycjaTabeliA pozycjaDruga = kursyAktualne[poz];
                    string format = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
                    x = pozycjaDruga.kurs_sredni.Replace(",", format);
                    kursSr = Convert.ToDouble(x);

                    double kwotaDocelowa = kwotaPLN / kursSr;

                    tbPrzeliczona.Text = kwotaDocelowa.ToString();
                }
        
            }catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                tbPrzeliczona.Text = "";
            }


        }

        private void lbxNaWalute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = lbxNaWalute.SelectedIndex;
            Windows.Storage.ApplicationData.Current.LocalSettings.Values["lbxNaWalute"] = index;
       



        }

        private void lbxZWaluty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = lbxZWaluty.SelectedIndex;
            Windows.Storage.ApplicationData.Current.LocalSettings.Values["lbxZWaluty"] = index;
         
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(OProgramie));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var indexWaluty = lbxZWaluty.SelectedIndex;
            var val = kursyAktualne[indexWaluty];
            var kod = val.kod_waluty;

            this.Frame.Navigate(typeof(Pomoc),kod);
        }

        private void aktualizuj_Click(object sender, RoutedEventArgs e)
        {
            InitComp();
        }

        private async void save()
        {
            StorageFile sampleFile = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("LastA.xml", CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(sampleFile, JsonConvert.SerializeObject(kursyAktualne));
        }

        private void adres_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

    public class PozycjaTabeliA
    {
        public string przelicznik { get; set; }
        public string kod_waluty { get; set; }
        public string kurs_sredni { get; set; }
    }
}
