using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int startX = 33;
        private static int startY = 10;
        private static List<Extrusion> instance = new List<Extrusion>();
        private static List<TextBox> lengthList = new List<TextBox>();
        private static List<TextBox> quantityList = new List<TextBox>();
        private static List<Parts> partsList = new List<Parts>();
        private static List<Parts> bar = new List<Parts>();
        private static List<CutResult> result = new List<CutResult>();
        private static int saw = 0;
        private static int extrusion_length = 0;
        private static int remnant = 99999;
        private class Parts
        {
            public int length;
            public int quantity;
            public bool enabled;
            public Parts(int length,int quantity)
            {
                this.length = length;
                this.quantity = quantity;
                this.enabled = true;
            }
        }
        private class Bar
        {
            public int index;
            public int length;
            public int quantity;
            public Bar(int index,int length, int quantity)
            {
                this.index = index;
                this.length = length;
                this.quantity = quantity;
            }
        }
        private class CutResult
        {
            public int remnant;
            public List<Bar> parts;
            public CutResult(int remnant, List<Bar> parts)
            {
                this.remnant = remnant;
                this.parts = new List<Bar>(parts);
            }
        }
        private void removePart(Parts part)
        {
            int index = partsList.IndexOf(part);
            partsList.RemoveAt(index);

        }
        private Parts getPart(int index)
        {
            return partsList[index];
        }
        private int getRemainingLength(List<Bar> bar)
        {
            int length = 0;
            foreach (Bar part in bar)
            {
                length += (part.length + saw) * part.quantity;
            }
            return extrusion_length - length;
        }
        private int getMinPartLength(List<Parts> parts)
        {
            int result = 0;
            foreach (Parts part in parts)
            {
                if (result == 0 || part.length < result)
                {
                    result = part.length;
                }
            }
            return result;
        }
        private Parts getpartWithBestRatio(List<Parts> parts, int length)
        {
            Parts result = null;
            int ratio = 0;
            int lowestRatio = 2;
            foreach(Parts part in parts)
            {
                if(needToCut(part) && length > part.length)
                {
                    ratio = (int)(length / (part.length + saw));
                    if(ratio < lowestRatio)
                    {
                        lowestRatio = ratio;
                        result = part;
                    }
                }
            }
            return result;
        }
        private List<Parts> getPartsToCut(List<int> calcuatedParts)
        {
            return partsList.FindAll((part) =>
            {
                int index = partsList.IndexOf(part);
                return needToCut(part) && (calcuatedParts[index] == null || calcuatedParts[index] < part.quantity);

            });
        }
        private bool needToCut(Parts part)
        {
            return part.enabled && part.quantity > 0 && part.length > 0;
        }
        private void Calcuate()
        {
            List<CutResult> result = new List<CutResult>();
            List<Bar> currentBar = new List<Bar>();
            List<int> calcuatedParts = new List<int>();
            for(int i=0;i< partsList.Count; i++)
            {
                calcuatedParts.Add(0);
                currentBar.Add(new Bar(i, partsList[i].length, 0));
            }
            List<Parts> partsToCut = getPartsToCut(calcuatedParts);
            int minpartLength = getMinPartLength(partsToCut);
            while (partsToCut.Count > 0)
            {
                int remaining = getRemainingLength(currentBar);
                while(remaining >= minpartLength && partsToCut.Count > 0)
                {
                    Parts part = null;
                    int index = 0;
                    part = getpartWithBestRatio(partsToCut, remaining);
                    if(part == null)
                    {
                        index = partsList.IndexOf(partsToCut[0]);
                        currentBar[index].quantity++;
                        
                    }
                    else
                    {
                        index = partsList.IndexOf(part);
                        currentBar[index].quantity++;
                    }
                    calcuatedParts[index]++;

                    remaining = getRemainingLength(currentBar);
                    partsToCut = getPartsToCut(calcuatedParts);
                    minpartLength = getMinPartLength(partsToCut);
                }
                currentBar.Sort((p0, p1) => p1.length - p0.length);
                result.Add(new CutResult(remaining, currentBar));
                currentBar = new List<Bar>();
                for (int i = 0; i < partsList.Count; i++)
                {
                    currentBar.Add(new Bar(i, partsList[i].length, 0));
                }
            }
            MainWindow.result = result;
        }
        private class Extrusion
        {
            public int length { get; set; }
           
        }
        
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            saw = Convert.ToInt32(thickness.Text.ToString());
            lengthList.Clear();
            quantityList.Clear();
            lengthList.Add(length1);
            lengthList.Add(length2);
            lengthList.Add(length3);
            lengthList.Add(length4);
            lengthList.Add(length5);
            lengthList.Add(length6);
            lengthList.Add(length7);
            lengthList.Add(length8);
            quantityList.Add(quantity1);
            quantityList.Add(quantity2);
            quantityList.Add(quantity3);
            quantityList.Add(quantity4);
            quantityList.Add(quantity5);
            quantityList.Add(quantity6);
            quantityList.Add(quantity7);
            quantityList.Add(quantity8);
            partsList.Clear();
            for (int i = 0; i < 8; i++)
            {
                int length = Convert.ToInt32(lengthList[i].Text.ToString());
                int quantity = Convert.ToInt32(quantityList[i].Text.ToString());
                if (length <= 0 || quantity <= 0)
                {
                    continue;
                }
                else
                {
                    partsList.Add(new Parts(length, quantity));
                }
            }
            partsList.Sort((p0,p1)=> p1.length - p0.length);
            int result_extrusion = 0;
            int count = 0;
            string result_str = string.Empty;
            foreach (Extrusion extrusion in instance)
            {
                extrusion_length = extrusion.length;
                Calcuate();
                int waste = 0;
                string str = string.Empty;
                foreach(CutResult cutResult in result)
                {
                    foreach(Bar bar in cutResult.parts)
                    {
                        if(bar.quantity != 0)
                            str += bar.length.ToString() +"X"+ bar.quantity.ToString()+"\t";
                    }
                    str += "waste : "+cutResult.remnant+"\n";
                    waste += cutResult.remnant;
                }
               
                if(waste <= remnant)
                {
                    result_str = string.Copy(str);
                    remnant = waste;
                    result_extrusion = extrusion_length;
                    count = result.Count;
                }
            }
            result_length.Text = result_extrusion.ToString();
            number_of_bars.Content = count.ToString();
            total_remnant.Content = remnant.ToString();
            result_textBlock.Text = result_str;
            result_str = string.Empty;
            remnant = 99999;
            
        }
        private void Extrusion_length_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                int text = Convert.ToInt32(extrusion_length_textBox.Text.ToString());
                instance.Add(new Extrusion() { length =  text});
                extrusion_Length_ListView.ItemsSource = instance;
                extrusion_Length_ListView.Items.Refresh();
                extrusion_length_textBox.Clear();
            }
        }

        private void Extrusion_Length_ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                ListBox listbox = (ListBox)sender;
                instance.RemoveAt(listbox.SelectedIndex);
                extrusion_Length_ListView.ItemsSource = instance;
                extrusion_Length_ListView.Items.Refresh();
            }
        }

        private void Btn_insert_Click(object sender, RoutedEventArgs e)
        {
            int text = Convert.ToInt32(extrusion_length_textBox.Text.ToString());
            instance.Add(new Extrusion() { length = text });
            extrusion_Length_ListView.ItemsSource = instance;
            extrusion_Length_ListView.Items.Refresh();
            extrusion_length_textBox.Clear();
        }
    }
}
