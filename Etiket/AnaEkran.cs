using ArgeMup.HazirKod;
using ArgeMup.HazirKod.Ekİşlemler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Etiket
{
    public partial class AnaEkran : Form
    {
        Point FareninİlkKonumu;
        bool GeçiciOlarakÇizdirme = true;

        public AnaEkran()
        {
            InitializeComponent();

            Text = "ArGeMuP " + Kendi.Adı + " " + Kendi.Sürümü_Dosya;

            Ayraç_Üst_Alt.SplitterDistance = Ayraç_Üst_Alt.Height * 15 / 50;
            Detaylar_YazıResim.Panel2Collapsed = true;
            Görsel_Eleman_Yazı_KarakterKümesi.Items.AddRange(FontFamily.Families.Select(f => f.Name).ToArray());
        }
        private void AnaEkran_Shown(object sender, EventArgs e)
        {
            IDepo_Eleman Şablonlar = Ortak.Depo_Ayarlar["Şablonlar"];
            for (int i = 0; i < Şablonlar.Elemanları.Length; i++)
            {
                Şablon_Şablonlar.Items.Add(Şablonlar.Elemanları[i].Adı, Şablonlar.Elemanları[i].Oku_Bit(null));
            }

            foreach (var biri in Değişkenler.Liste_UygulamaEkledi)
            {
                int satır = Tablo_Değişkenler.Rows.Add(new string[] { "Uygulama", biri.Key, biri.Value });
                Tablo_Değişkenler.Rows[satır].ReadOnly = true;
            }
            foreach (var biri in Değişkenler.Liste_KullanıcıEkledi)
            {
                Tablo_Değişkenler.Rows.Add(new string[] { "Siz", biri.Key, biri.Value });
            }

            Ortak.Görseller_YenidenHesaplat(Color.White);
            Ortak.Görseller_YenidenHesaplat(Ortak.YakınlaşmaOranı, (double)Görsel_Çıktı_Genişlik.Value, (double)Görsel_Çıktı_Yükseklik.Value);
            if (Şablon_Şablonlar.Items.Count > 0) Şablon_Şablonlar.SelectedIndex = 0;

            GeçiciOlarakÇizdirme = false;
            SağTuşMenü_Değişkenler_TuşlarıEkle();
            Görsel_Çizdir();
            Kaydet.Enabled = false;
        }
        private void AnaEkran_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Kaydet.Enabled)
            {
                DialogResult dr = MessageBox.Show("Değişiklikleri kaydetmeden çıkmak istiyor musunuz?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        #region Genel
        void Hata_Ekle(string Mesaj)
        {
            Hatalar.AppendText(Mesaj + Environment.NewLine);
            Hatalar.Select(Hatalar.Left, 0);

            if (Hatalar.Tag == null)
            {
                Hatalar.Tag = 0;
                Hatalar.BackColor = Color.Bisque;
            }
            else
            {
                Hatalar.Tag = null;
                Hatalar.BackColor = Color.LightPink;
            }
        }
        private void Kaydet_Click(object sender, EventArgs e)
        {
            #region Kullanıcı Değişkenlerinin kaydedilmesi
            Değişkenler.Liste_KullanıcıEkledi.Clear();
            for (int i = 0; i < Tablo_Değişkenler.RowCount; i++)
            {                                           //Adı                                              Ekleyen                                                                    
                if (((string)Tablo_Değişkenler[1, i].Value).BoşMu(true) || (string)Tablo_Değişkenler[0, i].Value != "Siz") continue;

                if (Değişkenler.Liste_KullanıcıEkledi.ContainsKey((string)Tablo_Değişkenler[1, i].Value))
                {
                    MessageBox.Show((string)Tablo_Değişkenler[1, i].Value + " adı ikinci kez girilmiş", Text);
                    return;
                }

                Değişkenler.Liste_KullanıcıEkledi.Add((string)Tablo_Değişkenler[1, i].Value, (string)Tablo_Değişkenler[2, i].Value);
            }
            Değişkenler.Kaydet();
            #endregion

            Şablon_Kaydet();

            File.WriteAllText(Ortak.Depo_Komut["Ayarlar", 0], Ortak.Depo_Ayarlar.YazıyaDönüştür());

            Kaydet.Enabled = false;
        }
        private void Sayfalar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Sayfalar.SelectedIndex != 0) 
            {
                //Şablon seçmeden diğer sekmelere gitmesin
                if (Şablon_Şablonlar.SelectedIndex < 0)
                {
                    Sayfalar.SelectedIndex = 0;
                    MessageBox.Show("Öncelikle bir şablon seçiniz veya oluşturunuz.", Text);
                }
            }
            else
            {
                if (Kaydet.Enabled)
                {
                    //şablon sayfasına değişiklik yapılarak geri dönüldü, kayıt
                    Şablon_Kaydet();
                }
            }

            //Yazıcı sayfası için yazıcıları güncellesin
            if (Sayfalar.SelectedIndex == 2)
            {
                if (Yazıcı_Yazıcılar.Items.Count < 1)
                {
                    for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
                    {
                        Yazıcı_Yazıcılar.Items.Add(PrinterSettings.InstalledPrinters[i]);
                    }
                }

                IDepo_Eleman yzc = Ortak.Depo_Şablon["Yazıcı"];
                if (!Yazıcı_Yazıcılar.Items.Contains(yzc.Oku(null, "Acayip Yazıcı")))
                {
                    Yazıcı_Açıklama.Text = "Kaydedilen yazıcı (" + yzc[0] + ") mevcut olmadığından seçilemedi";
                }
                else
                {
                    bool Kaydet_öncekidurum = Kaydet.Enabled;

                    Yazıcı_Yazıcılar.Text = yzc[0];
                    Yazıcı_Sol.Value = (decimal)yzc.Oku_Sayı(null, 5, 1);
                    Yazıcı_Üst.Value = (decimal)yzc.Oku_Sayı(null, 5, 2);
                    Yazıcı_Yazıcılar_SelectedIndexChanged(null, null);

                    Kaydet.Enabled = Kaydet_öncekidurum;
                }
            }
        }
        private void Tablo_Değişkenler_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (((string)Tablo_Değişkenler[0, e.RowIndex].Value).BoşMu(true)) Tablo_Değişkenler[0, e.RowIndex].Value = "Siz";

            if ((string)Tablo_Değişkenler[0, e.RowIndex].Value == "Siz")
            {
                Değişkenler.EkleVeyaDeğiştir((string)Tablo_Değişkenler[1, e.RowIndex].Value, (string)Tablo_Değişkenler[2, e.RowIndex].Value);

                //önceden kalma değişkenlerin silinmesi
                List<string> silinecekler = new List<string>();
                foreach (var biri in Değişkenler.Liste_KullanıcıEkledi)
                {
                    bool mevcut = false;
                    for (int i = 0; i < Tablo_Değişkenler.RowCount; i++)
                    {
                        if ((string)Tablo_Değişkenler[1, i].Value == biri.Key)
                        {
                            mevcut = true;
                            break;
                        }
                    }

                    if (!mevcut) silinecekler.Add(biri.Key);
                }
                foreach (var biri in silinecekler)
                {
                    Değişkenler.Liste_KullanıcıEkledi.Remove(biri);
                }

                SağTuşMenü_Değişkenler_TuşlarıEkle();
                Görsel_Çizdir();

                Kaydet.Enabled = true;
            }
        }
        void SağTuşMenü_Değişkenler_TuşlarıEkle()
        {
            SağTuşMenü_Değişkenler.Items.Clear();

            foreach (var biri in Değişkenler.Liste_UygulamaEkledi)
            {
                ToolStripItem tsi = SağTuşMenü_Değişkenler.Items.Add(biri.Key);
                tsi.Click += _SağTuşMenü_Değişkenler_Click;
            }

            foreach (var biri in Değişkenler.Liste_KullanıcıEkledi)
            {
                ToolStripItem tsi = SağTuşMenü_Değişkenler.Items.Add(biri.Key);
                tsi.Click += _SağTuşMenü_Değişkenler_Click;
            }

            void _SağTuşMenü_Değişkenler_Click(object senderr, EventArgs ee)
            {
                if (Ortak.SeçiliGörsel == null) return;

                ToolStripItem tsi_basılan = senderr as ToolStripItem;
                if (Ortak.SeçiliGörsel.Yazı_1_Resim_0) Görsel_Eleman_Yazı_İçerik.Text += "%" + tsi_basılan.Text + "%";
                else Görsel_Eleman_Resim_DosyaYolu.Text += "%" + tsi_basılan.Text + "%";
            }
        }
        #endregion

        #region Sayfa Şablon
        private void Şablon_Ekle_Click(object sender, EventArgs e)
        {
            if (Şablon_Adı.Text.BoşMu(true)) return;

            IDepo_Eleman şbl = Ortak.Depo_Ayarlar["Şablonlar"].Bul(Şablon_Adı.Text);
            if (şbl != null)
            {
                MessageBox.Show("Önceden kullanılmamaış bir isim seçiniz");
                return;
            }

            Ortak.Depo_Ayarlar["Şablonlar"].Yaz(Şablon_Adı.Text, true);
            Şablon_Şablonlar.Items.Add(Şablon_Adı.Text, true);

            Kaydet.Enabled = true;
        }
        private void Şablon_Kopyala_Click(object sender, EventArgs e)
        {
            if (Şablon_Adı.Text.BoşMu(true) || Şablon_Şablonlar.SelectedIndex < 0) return;
            
            IDepo_Eleman Şablonlar = Ortak.Depo_Ayarlar["Şablonlar"];
            IDepo_Eleman şbl = Şablonlar.Bul(Şablon_Adı.Text);
            if (şbl != null)
            {
                MessageBox.Show("Önceden kullanılmamaış bir isim seçiniz");
                return;
            }

            IDepo_Eleman yeni = Şablonlar[Şablon_Adı.Text];
            yeni.Yaz(null, true);
            yeni.Ekle(null, Şablonlar[Şablon_Şablonlar.Text].YazıyaDönüştür(null, true));

            Şablon_Şablonlar.Items.Add(Şablon_Adı.Text, true);

            Kaydet.Enabled = true;
        }
        private void Şablon_Sil_Click(object sender, EventArgs e)
        {
            if (Şablon_Şablonlar.SelectedIndex < 0) return;

            DialogResult dr = MessageBox.Show("Seçtiğiniz şablon silinecek.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.No) return;

            Ortak.Depo_Ayarlar["Şablonlar"].Sil(Şablon_Şablonlar.Text);
            Şablon_Şablonlar.Items.Remove(Şablon_Şablonlar.Text);

            Kaydet.Enabled = true;
        }

        private void Şablon_Şablonlar_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.Index < 0) return;

            string adı = Şablon_Şablonlar.Items[e.Index].ToString();
            Ortak.Depo_Ayarlar["Şablonlar"].Yaz(adı, e.NewValue == CheckState.Checked);

            Kaydet.Enabled = true;
        }
        private void Şablon_Şablonlar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Şablon_Şablonlar.SelectedIndex < 0) return;
            bool ÖncekiDeğer_Kaydet = Kaydet.Enabled;

            Ortak.Görseller_DizisiniOluştur(Ortak.Depo_Ayarlar["Şablonlar"][Şablon_Şablonlar.Text], false);
            Sayfa_Görsel.Text = "Görsel - " + Şablon_Şablonlar.Text;
            Sayfa_Yazıcı.Text = "Yazıcı - " + Şablon_Şablonlar.Text;

            Ortak.SeçiliGörsel = null;
            Ortak.Görseller_YenidenHesaplat(Ortak.Renge(Ortak.Depo_Şablon.Oku_BaytDizisi("Kağıt"), Color.White));
            Ortak.Görseller_YenidenHesaplat(Ortak.YakınlaşmaOranı, Ortak.Depo_Şablon.Oku_Sayı("Kağıt", 50, 1), Ortak.Depo_Şablon.Oku_Sayı("Kağıt", 30, 2));

            Görsel_Çıktı_ArkaPlanRenk.FlatAppearance.CheckedBackColor = Ortak.ArkaPlanRengi;
            Görsel_Çıktı_ArkaPlanRenk.Checked = Ortak.ArkaPlanRengi != Color.Transparent;
            Görsel_Çıktı_Genişlik.Value = (decimal)Ortak.KullanılabilirAlan_mm.Width;
            Görsel_Çıktı_Yükseklik.Value = (decimal)Ortak.KullanılabilirAlan_mm.Height;

            Yazıcı_Yazıcılar.SelectedIndex = -1;
            Yazıcı_Sol.Value = (decimal)Ortak.Depo_Şablon["Yazıcı"].Oku_Sayı(null, 5, 1);
            Yazıcı_Üst.Value = (decimal)Ortak.Depo_Şablon["Yazıcı"].Oku_Sayı(null, 5, 2);

            Görsel_Çıktı_BoyutlarıDeğişti(null, null);
            Görsel_Çıktı_Yakınlaştırma.Value = 35;
            Görsel_Çıktı_Yakınlaştırma_Scroll(null, null);

            Görsel_Elemanlar.Items.Clear();
            foreach (Ortak.Görsel biri in Ortak.Görsel_ler)
            {
                Görsel_Elemanlar.Items.Add(biri.Adı, biri.Görünsün);
            }

            Hatalar.Text = null;
            Kaydet.Enabled = ÖncekiDeğer_Kaydet;
        }

        void Şablon_Kaydet()
        {
            if (Ortak.Depo_Şablon == null) return;

            Ortak.Depo_Şablon.Sil("Görseller", true, true);
            foreach (Ortak.Görsel biri in Ortak.Görsel_ler)
            {
                biri.Depo_Kaydet(Ortak.Depo_Şablon);
            }

            Ortak.Depo_Şablon.Yaz("Kağıt", Ortak.Renkten(Ortak.ArkaPlanRengi), 0);
            Ortak.Depo_Şablon.Yaz("Kağıt", (double)Görsel_Çıktı_Genişlik.Value, 1);
            Ortak.Depo_Şablon.Yaz("Kağıt", (double)Görsel_Çıktı_Yükseklik.Value, 2);

            if (Yazıcı_Yazıcılar.Text.DoluMu()) Ortak.Depo_Şablon.Yaz("Yazıcı", Yazıcı_Yazıcılar.Text);
            Ortak.Depo_Şablon.Yaz("Yazıcı", (double)Yazıcı_Sol.Value, 1);
            Ortak.Depo_Şablon.Yaz("Yazıcı", (double)Yazıcı_Üst.Value, 2);
        }
        #endregion

        #region Sayfa Görsel
        private void Görsel_Ayar_Değişti(object sender, EventArgs e)
        {
            if (Ortak.SeçiliGörsel == null) return;

            //geçerli bilgileri geçerli eleman içine kaydet
            if (Görsel_Eleman_Adı.Text != Ortak.SeçiliGörsel.Adı)
            {
                if (Ortak.Görseller_Görseli_Bul(Görsel_Eleman_Adı.Text) != null)
                {
                    Hata_Ekle(Ortak.SeçiliGörsel.Adı + " -> Yeni girdiğiniz (" + Görsel_Eleman_Adı.Text + ") adı kullanıldığından işleme alınmadı");
                }
                else
                {
                    Ortak.Görsel gecici = Ortak.SeçiliGörsel;
                    Ortak.SeçiliGörsel = null;

                    int konumu = Görsel_Elemanlar.Items.IndexOf(gecici.Adı);
                    Görsel_Elemanlar.Items.Insert(konumu, Görsel_Eleman_Adı.Text);
                    Görsel_Elemanlar.Items.RemoveAt(konumu + 1);
                    Görsel_Elemanlar.SetItemChecked(konumu, gecici.Görünsün);
                    Görsel_Elemanlar.SelectedIndex = konumu;

                    gecici.Adı = Görsel_Eleman_Adı.Text;

                    Ortak.SeçiliGörsel = gecici;
                }
            }
            Ortak.SeçiliGörsel.Açı = (float)Görsel_Eleman_Açı.Value;
            Ortak.SeçiliGörsel.Çerçeve.X = (float)Görsel_Eleman_Sol.Value;
            Ortak.SeçiliGörsel.Çerçeve.Y = (float)Görsel_Eleman_Üst.Value;
            Ortak.SeçiliGörsel.Çerçeve.Width = (float)Görsel_Eleman_Genişlik.Value;
            Ortak.SeçiliGörsel.Çerçeve.Height = (float)Görsel_Eleman_Yükseklik.Value;
            Ortak.SeçiliGörsel.EtKalınlığı = (float)Görsel_Eleman_EtKalınlığı.Value;
            Ortak.SeçiliGörsel.Renk = Görsel_Eleman_Renk.BackColor;
            Ortak.SeçiliGörsel.Renk_ArkaPlan = Görsel_Eleman_Renk_Arkaplan.FlatAppearance.CheckedBackColor;

            if (Ortak.SeçiliGörsel.Yazı_1_Resim_0)
            {
                Ortak.SeçiliGörsel.Yazı_KarakterKümesi = Görsel_Eleman_Yazı_KarakterKümesi.Text;
                Ortak.SeçiliGörsel.Yazıİçeriği_Veya_ResimDosyaYolu = Görsel_Eleman_Yazı_İçerik.Text;
                Ortak.SeçiliGörsel.Yazı_Kalın = Görsel_Eleman_Yazı_Kalın.Checked;
                Ortak.SeçiliGörsel.Yazı_Yaslama_Yatay = Görsel_Eleman_Yazı_Yaslama_Yatay.SelectedIndex;
                Ortak.SeçiliGörsel.Yazı_Yaslama_Dikey = Görsel_Eleman_Yazı_Yaslama_Dikey.SelectedIndex;
            }
            else
            {
                Ortak.SeçiliGörsel.Yazıİçeriği_Veya_ResimDosyaYolu = Görsel_Eleman_Resim_DosyaYolu.Text;
                Ortak.SeçiliGörsel.Resim_Yuvarlama_Çap = (float)Görsel_Eleman_Resim_ÇemberÇap.Value;
            }

            Ortak.SeçiliGörsel.YenidenHesaplat();
            Görsel_Çizdir();

            Kaydet.Enabled = true;
        }

        private void Görsel_Elemanlar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Görsel_Elemanlar.SelectedIndex < 0) return;
            
            Ortak.Görsel şimdi_seçilen = Ortak.Görseller_Görseli_Bul(Görsel_Elemanlar.Text);
            if (şimdi_seçilen == null || şimdi_seçilen == Ortak.SeçiliGörsel) return; //aynı satırı seçerse çık
            Ortak.SeçiliGörsel = null; //tekrar tekrar çizdirmemesi için
            
            Görsel_Eleman_Adı.Text = şimdi_seçilen.Adı;
            Görsel_Eleman_Açı.Value = (decimal)şimdi_seçilen.Açı;
            Görsel_Eleman_Sol.Value = (decimal)şimdi_seçilen.Çerçeve.X;
            Görsel_Eleman_Üst.Value = (decimal)şimdi_seçilen.Çerçeve.Y;
            Görsel_Eleman_Genişlik.Value = (decimal)şimdi_seçilen.Çerçeve.Width;
            Görsel_Eleman_Yükseklik.Value = (decimal)şimdi_seçilen.Çerçeve.Height;
            Görsel_Eleman_EtKalınlığı.Value = (decimal)şimdi_seçilen.EtKalınlığı;
            Görsel_Eleman_Renk.BackColor = şimdi_seçilen.Renk;
            Görsel_Eleman_Renk_Arkaplan.FlatAppearance.CheckedBackColor = şimdi_seçilen.Renk_ArkaPlan;
            Görsel_Eleman_Renk_Arkaplan.Checked = şimdi_seçilen.Renk_ArkaPlan != Color.Transparent;
            if (şimdi_seçilen.Yazı_1_Resim_0)
            {
                Detaylar_YazıResim.Panel1Collapsed = false;
                Detaylar_YazıResim.Panel2Collapsed = true;

                if (!Görsel_Eleman_Yazı_KarakterKümesi.Items.Contains(şimdi_seçilen.Yazı_KarakterKümesi))
                {
                    Hata_Ekle(Görsel_Elemanlar.Text + " -> Kayıtlı olan (" + şimdi_seçilen.Yazı_KarakterKümesi + ") mevcut karakter kümeleri arasında bulunmadığından açılamadı");
                    //Görsel_Eleman_Yazı_KarakterKümesi.SelectedIndex = 0;
                }
                Görsel_Eleman_Yazı_KarakterKümesi.Text = şimdi_seçilen.Yazı_KarakterKümesi;
                Görsel_Eleman_Yazı_İçerik.Text = şimdi_seçilen.Yazıİçeriği_Veya_ResimDosyaYolu;
                Görsel_Eleman_Yazı_Kalın.Checked = şimdi_seçilen.Yazı_Kalın;
                Görsel_Eleman_Yazı_Yaslama_Yatay.SelectedIndex = şimdi_seçilen.Yazı_Yaslama_Yatay;
                Görsel_Eleman_Yazı_Yaslama_Dikey.SelectedIndex = şimdi_seçilen.Yazı_Yaslama_Dikey;
            }
            else
            {
                Detaylar_YazıResim.Panel1Collapsed = true;
                Detaylar_YazıResim.Panel2Collapsed = false;

                Görsel_Eleman_Resim_DosyaYolu.Text = şimdi_seçilen.Yazıİçeriği_Veya_ResimDosyaYolu;
                if (Görsel_Eleman_Resim_DosyaYolu.Text.DoluMu())
                {
                    if (!File.Exists(şimdi_seçilen.Yazıİçeriği_Veya_ResimDosyaYolu_Çözümlenmiş)) Hata_Ekle(Görsel_Elemanlar.Text + " -> Resim dosyası açılamadı (" + Görsel_Eleman_Resim_DosyaYolu.Text + ") " + şimdi_seçilen.Yazıİçeriği_Veya_ResimDosyaYolu_Çözümlenmiş);
                }
                Görsel_Eleman_Resim_ÇemberÇap.Value = (decimal)şimdi_seçilen.Resim_Yuvarlama_Çap;
            }

            Ortak.SeçiliGörsel = şimdi_seçilen;
            Görsel_Çizdir();
        }
        private void Görsel_Elemanlar_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.Index < 0) return; 

            string adı = Görsel_Elemanlar.Items[e.Index].ToString();
            Ortak.Görsel g = Ortak.Görseller_Görseli_Bul(adı);
            
            if (g == null) return;
            g.Görünsün = e.NewValue == CheckState.Checked;

            Görsel_Çizdir();
            Kaydet.Enabled = true;
        }
        
        private void Görsel_Eleman_Yukarı_Click(object sender, EventArgs e)
        {
            if (Görsel_Elemanlar.SelectedIndex < 1) return;
            Ortak.SeçiliGörsel = null;

            Ortak.Görsel a = Ortak.Görseller_Görseli_Bul(Görsel_Elemanlar.Text);
            List<Ortak.Görsel> l = Ortak.Görsel_ler.ToList();
            l.Remove(a);
            l.Insert(Görsel_Elemanlar.SelectedIndex - 1, a);
            Ortak.Görsel_ler = l.ToArray();

            Görsel_Elemanlar.Items.Insert(Görsel_Elemanlar.SelectedIndex - 1, a.Adı);
            int yeni_konum = Görsel_Elemanlar.SelectedIndex - 2;
            Görsel_Elemanlar.SetItemChecked(yeni_konum, a.Görünsün);
            Görsel_Elemanlar.Items.RemoveAt(Görsel_Elemanlar.SelectedIndex);

            Görsel_Çizdir();

            Görsel_Elemanlar.SelectedIndex = yeni_konum;
            Kaydet.Enabled = true;
        }
        private void Görsel_Eleman_Aşağı_Click(object sender, EventArgs e)
        {
            if (Görsel_Elemanlar.SelectedIndex < 0 || Görsel_Elemanlar.SelectedIndex >= (Görsel_Elemanlar.Items.Count - 1)) return;
            Ortak.SeçiliGörsel = null;

            Ortak.Görsel a = Ortak.Görseller_Görseli_Bul(Görsel_Elemanlar.Text);
            List<Ortak.Görsel> l = Ortak.Görsel_ler.ToList();
            l.Remove(a);
            l.Insert(Görsel_Elemanlar.SelectedIndex + 1, a);
            Ortak.Görsel_ler = l.ToArray();

            Görsel_Elemanlar.Items.Insert(Görsel_Elemanlar.SelectedIndex + 2, a.Adı);
            int yeni_konum = Görsel_Elemanlar.SelectedIndex + 2;
            Görsel_Elemanlar.SetItemChecked(yeni_konum, a.Görünsün);
            Görsel_Elemanlar.Items.RemoveAt(Görsel_Elemanlar.SelectedIndex);

            Görsel_Çizdir();

            Görsel_Elemanlar.SelectedIndex = yeni_konum - 1;
            Kaydet.Enabled = true;
        }
        private void Görsel_Eleman_Sil_Click(object sender, EventArgs e)
        {
            if (Görsel_Elemanlar.SelectedIndex < 0) return;

            if (Ortak.Depo_Komut.Oku("Ön Tanımlı Görseller/" + Görsel_Elemanlar.Text) != null)
            {
                MessageBox.Show("Üst uygulamanın ön tanımlı olarak eklediği bir görsel olduğundan silinemez." + Environment.NewLine +
                    "Silmek yerine soldaki kutucuğun onayını kaldırıp görünmez yapabilirsiniz.", Text);
                return;
            }

            DialogResult dr = MessageBox.Show("Seçtiğiniz görsel silinecek.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.No) return;

            Ortak.Görsel a = Ortak.Görseller_Görseli_Bul(Görsel_Elemanlar.Text);
            List<Ortak.Görsel> l = Ortak.Görsel_ler.ToList();
            l.Remove(a);
            Ortak.Görsel_ler = l.ToArray();

            Görsel_Elemanlar.Items.RemoveAt(Görsel_Elemanlar.SelectedIndex);

            Ortak.SeçiliGörsel = null;
            Görsel_Çizdir();
            Kaydet.Enabled = true;
        }
        private void Görsel_Eleman_Resim_Click(object sender, EventArgs e)
        {
            string adı = null;
            for (int i = 1; i < 1000; i++)
            {
                adı = "Resim " + i;
                if (Ortak.Görseller_Görseli_Bul(adı) == null) break;
            }

            Array.Resize(ref Ortak.Görsel_ler, Ortak.Görsel_ler.Length + 1);
            Ortak.Görsel_ler[Ortak.Görsel_ler.Length - 1] = new Ortak.Görsel(null)
            {
                Adı = adı,
                Yazı_1_Resim_0 = false,
                Görünsün = true,
                Açı = 0,
                Renk = Color.Black,
                Renk_ArkaPlan = Color.Yellow,
                EtKalınlığı = 1,
                Resim_Yuvarlama_Çap = 5,
                Çerçeve = new RectangleF(0, 0, 20, 20),
                Yazıİçeriği_Veya_ResimDosyaYolu = null,
            };

            int konum = Görsel_Elemanlar.Items.Add(adı, true);
            Görsel_Elemanlar.SelectedIndex = konum;

            Görsel_Çizdir();
            Kaydet.Enabled = true;
        }
        private void Görsel_Eleman_Yazı_Click(object sender, EventArgs e)
        {
            string adı = null;
            for (int i = 1; i < 1000; i++)
            {
                adı = "Yazı " + i;
                if (Ortak.Görseller_Görseli_Bul(adı) == null) break;
            }

            Array.Resize(ref Ortak.Görsel_ler, Ortak.Görsel_ler.Length + 1);
            Ortak.Görsel_ler[Ortak.Görsel_ler.Length - 1] = new Ortak.Görsel(null)
            {
                Adı = adı,
                Yazı_1_Resim_0 = true,
                Görünsün = true,
                Açı = 0,
                Çerçeve = new RectangleF(0, 0, 10, 5),
                Yazıİçeriği_Veya_ResimDosyaYolu = "Yeni Yazı",
                Yazı_KarakterKümesi = "Calibri",
                EtKalınlığı = 0,
                Renk = Color.Black,
                Renk_ArkaPlan = Color.Yellow,
                Yazı_Kalın = false,
                Yazı_Yaslama_Yatay = 1,
                Yazı_Yaslama_Dikey = 1
            };

            int konum = Görsel_Elemanlar.Items.Add(adı, true);
            Görsel_Elemanlar.SelectedIndex = konum;

            Görsel_Çizdir();
            Kaydet.Enabled = true;
        }

        private void Görsel_Eleman_Renk_Click(object sender, EventArgs e)
        {
            if (Ortak.SeçiliGörsel == null || RenkSeçici.ShowDialog() == DialogResult.Cancel) return;

            Ortak.SeçiliGörsel.Renk = RenkSeçici.Color;
            Görsel_Eleman_Renk.BackColor = RenkSeçici.Color;

            Ortak.SeçiliGörsel.YenidenHesaplat();
            Görsel_Çizdir();

            Kaydet.Enabled = true;
        }
        private void Görsel_Eleman_Renk_Arkaplan_CheckedChanged(object sender, EventArgs e)
        {
            if (Ortak.SeçiliGörsel == null) return;

            if (Görsel_Eleman_Renk_Arkaplan.Checked)
            {
                if (RenkSeçici.ShowDialog() == DialogResult.Cancel)
                {
                    Görsel_Eleman_Renk_Arkaplan.Checked = false;
                    return;
                }

                Ortak.SeçiliGörsel.Renk_ArkaPlan = RenkSeçici.Color;
            }
            else
            {
                Ortak.SeçiliGörsel.Renk_ArkaPlan = Color.Transparent;
            }

            Görsel_Eleman_Renk_Arkaplan.FlatAppearance.CheckedBackColor = Ortak.SeçiliGörsel.Renk_ArkaPlan;
            Ortak.SeçiliGörsel.YenidenHesaplat();
            Görsel_Çizdir();

            Kaydet.Enabled = true;
        }

        private void Görsel_Çıktı_MouseDown(object sender, MouseEventArgs e)
        {
            if (Ortak.Görsel_ler == null) return;

            foreach (Ortak.Görsel biri in Ortak.Görsel_ler)
            {
                if (!biri.Görünsün) continue;

                PointF nokta = new PointF(e.Location.X * 0.254f / Ortak.YakınlaşmaOranı, e.Location.Y * 0.254f / Ortak.YakınlaşmaOranı);
                RectangleF elm = new RectangleF(biri.Çerçeve.Location, new SizeF(biri.Çerçeve.Width, biri.Çerçeve.Height));

                if (elm.Contains(nokta))
                {
                    int konumu = Görsel_Elemanlar.Items.IndexOf(biri.Adı);
                    Görsel_Elemanlar.SelectedIndex = konumu;

                    FareninİlkKonumu = e.Location;
                    return;
                }
            }

            Görsel_SeçiliOlanı_Kaldır();
        }
        private void Görsel_Çıktı_MouseMove(object sender, MouseEventArgs e)
        {
            if (Ortak.Görsel_ler == null || Ortak.SeçiliGörsel == null || !Görsel_Çıktı_Sürüklenebilir.Checked) return;
            Ortak.Görsel gecici = Ortak.SeçiliGörsel;
            Ortak.SeçiliGörsel = null;

            PointF fark = PointF.Subtract(e.Location, new SizeF(FareninİlkKonumu.X, FareninİlkKonumu.Y));
            FareninİlkKonumu = e.Location;
            fark = new PointF(fark.X * 0.254f / Ortak.YakınlaşmaOranı, fark.Y * 0.254f / Ortak.YakınlaşmaOranı);

            if (e.Button == MouseButtons.Left)
            {
                fark = PointF.Add(gecici.Çerçeve.Location, new SizeF(fark));
                gecici.Çerçeve.Location = fark;
            }
            else if (e.Button == MouseButtons.Right)
            {
                SizeF yeniBoyutu = gecici.Çerçeve.Size + new SizeF(fark);
                if (yeniBoyutu.Width > 1 && yeniBoyutu.Height > 1) gecici.Çerçeve.Size = yeniBoyutu;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                RectangleF yeniBoyutu = gecici.Çerçeve;
                yeniBoyutu.Inflate(new SizeF(fark));
                if (yeniBoyutu.Width > 1 && yeniBoyutu.Height > 1) gecici.Çerçeve = yeniBoyutu;
            }
            else
            {
                Ortak.SeçiliGörsel = gecici;
                return;
            }

            Görsel_Eleman_Sol.Value = (decimal)gecici.Çerçeve.Location.X;
            Görsel_Eleman_Üst.Value = (decimal)gecici.Çerçeve.Location.Y;
            Görsel_Eleman_Genişlik.Value = (decimal)gecici.Çerçeve.Size.Width;
            Görsel_Eleman_Yükseklik.Value = (decimal)gecici.Çerçeve.Size.Height;

            Ortak.SeçiliGörsel = gecici;
            Görsel_Ayar_Değişti(null, null);
        }
        private void Görsel_Çıktı_BoyutlarıDeğişti(object sender, EventArgs e)
        {
            Kaydet.Enabled = true;
            Ortak.Görseller_YenidenHesaplat(Ortak.YakınlaşmaOranı, (double)Görsel_Çıktı_Genişlik.Value, (double)Görsel_Çıktı_Yükseklik.Value);

            Görsel_Çizdir();
        }
        private void Görsel_Çıktı_ArkaPlanRenk_CheckedChanged(object sender, EventArgs e)
        {
            Color Seçilen;
            if (Görsel_Çıktı_ArkaPlanRenk.Checked)
            {
                if (RenkSeçici.ShowDialog() == DialogResult.Cancel)
                {
                    Görsel_Çıktı_ArkaPlanRenk.Checked = false;
                    return;
                }

                Seçilen = RenkSeçici.Color;
            }
            else
            {
                Seçilen = Color.Transparent;
            }

            Görsel_Çıktı_ArkaPlanRenk.FlatAppearance.CheckedBackColor = Seçilen;
            Ortak.Görseller_YenidenHesaplat(Seçilen);

            Görsel_Çizdir();

            Kaydet.Enabled = true;
        }
        private void Görsel_Çıktı_Yakınlaştırma_Scroll(object sender, EventArgs e)
        {
            Ortak.Görseller_YenidenHesaplat(Görsel_Çıktı_Yakınlaştırma.Value * 0.1f, Ortak.KullanılabilirAlan_mm.Width, Ortak.KullanılabilirAlan_mm.Height);

            Görsel_Çizdir();
        }

        private void Görsel_Çıktı_Panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) Görsel_SeçiliOlanı_Kaldır();
            else
            {
                if (Görsel_Çıktı.Image == null) return;

                DialogResult dr = DosyayaKaydetPaneli.ShowDialog(this);
                if (dr == DialogResult.Cancel) return;

                Görsel_Çıktı.Image.Save(DosyayaKaydetPaneli.FileName);
            }
        }

        void Görsel_SeçiliOlanı_Kaldır()
        {
            Görsel_Ayar_Değişti(null, null);
            Ortak.SeçiliGörsel = null;
            Görsel_Çizdir();
            Görsel_Elemanlar.SelectedIndex = -1;
        }
        void Görsel_Çizdir()
        {
            if (GeçiciOlarakÇizdirme) return;

            if (Görsel_Çıktı.Image != null) Görsel_Çıktı.Image.Dispose();
            string Hatalar = Ortak.Görseller_Görseli_ResimHalineGetir(out Image Resim);
            Görsel_Çıktı.Image = Resim;
            Görsel_Çıktı.Size = Resim.Size;

            if (Hatalar.DoluMu()) Hata_Ekle(Hatalar);
        }
        #endregion

        #region Sayfa Yazıcı
        private void Yazıcı_Ayar_Değişti(object sender, EventArgs e)
        {
            Kaydet.Enabled = true;
        }
        private void Yazıcı_Yazıcılar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Yazıcı_Yazıcılar.SelectedIndex < 0) return;

            Application.DoEvents();
            PrinterSettings ps = new PrinterSettings();
            ps.PrinterName = Yazıcı_Yazıcılar.Text;

            Size KullanılabilirAlan_piksel = new Size((int)ps.DefaultPageSettings.PrintableArea.Size.Width, (int)ps.DefaultPageSettings.PrintableArea.Size.Height);
            PointF SolÜstKöşe_mm = new PointF(ps.DefaultPageSettings.PrintableArea.Location.X * 0.254f, ps.DefaultPageSettings.PrintableArea.Location.Y * 0.254f);
            string SeçilenKağıt_Adı = ps.DefaultPageSettings.PaperSize.PaperName;

            Yazıcı_Açıklama.Text = "Seçilen Kağıt " + SeçilenKağıt_Adı + " (" + (int)(KullanılabilirAlan_piksel.Width * 0.254) + " mm x " + (int)(KullanılabilirAlan_piksel.Height * 0.254) + " mm)" + Environment.NewLine +
                "Soldan ve sağdan " + (int)(SolÜstKöşe_mm.X) + " mm, üstten ve alttan " + (int)(SolÜstKöşe_mm.Y) + " mm boşluk bırakılması önerilmiş.";

            Yazıcı_Ayar_Değişti(null, null);
        }
        private void Yazıcı_Yazdır_Click(object sender, EventArgs e)
        {
            float gecici_yakınlık = Ortak.YakınlaşmaOranı;
            Ortak.YakınlaşmaOranı = 1;

            string snç = Ortak.Görseller_Görseli_Yazdır();
            if (snç.DoluMu()) Yazıcı_Açıklama.Text += Environment.NewLine + snç;

            Ortak.YakınlaşmaOranı = gecici_yakınlık;
        }
        #endregion
    }
}