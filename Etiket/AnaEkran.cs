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

        public AnaEkran()
        {
            InitializeComponent();

            Text = "ArGeMuP " + Kendi.Adı + " " + Kendi.Sürüm;

            Görsel_Kat1_Kat2.SplitterDistance = Görsel_Kat1_Kat2.Height / 4;
            Detaylar_YazıResim.Panel2Collapsed = true;
            Görsel_Eleman_Yazı_KarakterKümesi.Items.AddRange(FontFamily.Families.Select(f => f.Name).ToArray());
        }
        private void AnaEkran_Shown(object sender, EventArgs e)
        {
            Ortak.ArkaPlanRengi = Ortak.Renge(Ortak.Depo_Ayarlar.Oku_BaytDizisi("Kağıt"), Color.White);
            Görsel_Çıktı_ArkaPlanRenk.FlatAppearance.CheckedBackColor = Ortak.ArkaPlanRengi;
            Görsel_Çıktı_ArkaPlanRenk.Checked = Ortak.ArkaPlanRengi != Color.Transparent;
            
            Görsel_Çıktı_Genişlik.Value = (decimal)Ortak.Depo_Ayarlar.Oku_Sayı("Kağıt", 50, 1);
            Görsel_Çıktı_Yükseklik.Value = (decimal)Ortak.Depo_Ayarlar.Oku_Sayı("Kağıt", 30, 2);
            
            Yazıcı_Sol.Value = (decimal)Ortak.Depo_Ayarlar["Yazıcı"].Oku_Sayı(null, 5, 1);
            Yazıcı_Üst.Value = (decimal)Ortak.Depo_Ayarlar["Yazıcı"].Oku_Sayı(null, 5, 2);

            Görsel_Çıktı_BoyutlarıDeğişti(null, null);
            Görsel_Çıktı_Yakınlaştırma.Value = 35;
            Görsel_Çıktı_Yakınlaştırma_Scroll(null, null);

            foreach (Ortak.Görsel biri in Ortak.Görsel_ler)
            {
                Görsel_Elemanlar.Items.Add(biri.Adı, biri.Görünsün);
            }

            Çizdir();
            Kaydet.Enabled = false;
        }
        private void AnaEkran_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Kaydet.Enabled)
            {
                DialogResult dr = MessageBox.Show("Değişiklikleri kaydetmeden çıkmak istiyor musunuz?", Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
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
            Ortak.Depo_Ayarlar.Sil("Görseller", true, true);
            foreach (Ortak.Görsel biri in Ortak.Görsel_ler)
            {
                biri.Depo_Kaydet(Ortak.Depo_Ayarlar);
            }

            Ortak.Depo_Ayarlar.Yaz("Kağıt", Ortak.Renkten(Ortak.ArkaPlanRengi), 0);
            Ortak.Depo_Ayarlar.Yaz("Kağıt", (double)Görsel_Çıktı_Genişlik.Value, 1);
            Ortak.Depo_Ayarlar.Yaz("Kağıt", (double)Görsel_Çıktı_Yükseklik.Value, 2);

            if (Yazıcı_Yazıcılar.Text.DoluMu()) Ortak.Depo_Ayarlar.Yaz("Yazıcı", Yazıcı_Yazıcılar.Text);
            Ortak.Depo_Ayarlar.Yaz("Yazıcı", (double)Yazıcı_Sol.Value, 1);
            Ortak.Depo_Ayarlar.Yaz("Yazıcı", (double)Yazıcı_Üst.Value, 2);

            File.WriteAllText(Ortak.Depo_Komut["Ayarlar", 0], Ortak.Depo_Ayarlar.YazıyaDönüştür());

            Kaydet.Enabled = false;
        }
        private void Sayfalar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Sayfalar.SelectedIndex == 1 && Yazıcı_Yazıcılar.Items.Count == 0)
            {
                for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
                {
                    Yazıcı_Yazıcılar.Items.Add(PrinterSettings.InstalledPrinters[i]);
                }

                IDepo_Eleman yzc = Ortak.Depo_Ayarlar["Yazıcı"];
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

        void Çizdir()
        {
            if (Görsel_Çıktı.Image != null) Görsel_Çıktı.Image.Dispose();
            string Hatalar = Ortak.Görseller_Görseli_ResimHalineGetir(out Image Resim);
            Görsel_Çıktı.Image = Resim;
            Görsel_Çıktı.Size = Resim.Size;

            if (Hatalar.DoluMu()) Hata_Ekle(Hatalar);
        }

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
            Çizdir();

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
                    Hata_Ekle(Görsel_Elemanlar.Text + " -> Kayıtlı olan (" + şimdi_seçilen.Yazı_KarakterKümesi + ") mevcut karakter kümeleri arasında bulunmadığından değiştirildi");
                    Görsel_Eleman_Yazı_KarakterKümesi.SelectedIndex = 0;
                }
                else Görsel_Eleman_Yazı_KarakterKümesi.Text = şimdi_seçilen.Yazı_KarakterKümesi;
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
                if (Görsel_Eleman_Resim_DosyaYolu.Text.DoluMu() && !File.Exists(Görsel_Eleman_Resim_DosyaYolu.Text))
                {
                    Hata_Ekle(Görsel_Elemanlar.Text + " -> Resim dosyası açılamadı " + şimdi_seçilen.Yazıİçeriği_Veya_ResimDosyaYolu);
                }
                Görsel_Eleman_Resim_ÇemberÇap.Value = (decimal)şimdi_seçilen.Resim_Yuvarlama_Çap;
            }

            Ortak.SeçiliGörsel = şimdi_seçilen;
            Çizdir();
        }
        private void Görsel_Elemanlar_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.Index < 0) return; 

            string adı = Görsel_Elemanlar.Items[e.Index].ToString();
            Ortak.Görsel gecici = Ortak.Görseller_Görseli_Bul(adı);
            gecici.Görünsün = e.NewValue == CheckState.Checked;

            Çizdir();
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

            Çizdir();

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

            Çizdir();

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

            DialogResult dr = MessageBox.Show("Seçtiğiniz görsel silinecek.", Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Cancel) return;

            Ortak.Görsel a = Ortak.Görseller_Görseli_Bul(Görsel_Elemanlar.Text);
            List<Ortak.Görsel> l = Ortak.Görsel_ler.ToList();
            l.Remove(a);
            Ortak.Görsel_ler = l.ToArray();

            Görsel_Elemanlar.Items.RemoveAt(Görsel_Elemanlar.SelectedIndex);

            Ortak.SeçiliGörsel = null;
            Çizdir();
            Kaydet.Enabled = true;
        }
        private void Görsel_Eleman_Resim_Click(object sender, EventArgs e)
        {
            string adı = "Resim " + Path.GetRandomFileName();
            while (Ortak.Görseller_Görseli_Bul(adı) != null) adı = "Resim " + Path.GetRandomFileName();

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
                Yazıİçeriği_Veya_ResimDosyaYolu = "Buraya resminizin dosya yolunu giriniz",
            };

            int konum = Görsel_Elemanlar.Items.Add(adı, true);
            Görsel_Elemanlar.SelectedIndex = konum;

            Çizdir();
            Kaydet.Enabled = true;
        }
        private void Görsel_Eleman_Yazı_Click(object sender, EventArgs e)
        {
            string adı = "Yazı " + Path.GetRandomFileName();
            while (Ortak.Görseller_Görseli_Bul(adı) != null) adı = "Yazı " + Path.GetRandomFileName();

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

            Çizdir();
            Kaydet.Enabled = true;
        }

        private void Görsel_Eleman_Renk_Click(object sender, EventArgs e)
        {
            if (Ortak.SeçiliGörsel == null || RenkSeçici.ShowDialog() == DialogResult.Cancel) return;

            Ortak.SeçiliGörsel.Renk = RenkSeçici.Color;
            Görsel_Eleman_Renk.BackColor = RenkSeçici.Color;

            Ortak.SeçiliGörsel.YenidenHesaplat();
            Çizdir();

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
            Çizdir();

            Kaydet.Enabled = true;
        }

        private void Görsel_Çıktı_MouseDown(object sender, MouseEventArgs e)
        {
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

            Görsel_Ayar_Değişti(null, null);
            Ortak.SeçiliGörsel = null;
            Çizdir();
            Görsel_Elemanlar.SelectedIndex = -1;
        }
        private void Görsel_Çıktı_MouseMove(object sender, MouseEventArgs e)
        {
            if (Ortak.SeçiliGörsel == null || !Görsel_Çıktı_Sürüklenebilir.Checked) return;
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
            Ortak.KullanılabilirAlan_mm = new SizeF((float)Görsel_Çıktı_Genişlik.Value, (float)Görsel_Çıktı_Yükseklik.Value);
            Ortak.KullanılabilirAlan_piksel_Resim = new Size((int)(Ortak.KullanılabilirAlan_mm.Width * Ortak.YakınlaşmaOranı / 0.254), (int)(Ortak.KullanılabilirAlan_mm.Height * Ortak.YakınlaşmaOranı / 0.254));

            Çizdir();
        }
        private void Görsel_Çıktı_ArkaPlanRenk_CheckedChanged(object sender, EventArgs e)
        {
            if (Görsel_Çıktı_ArkaPlanRenk.Checked)
            {
                if (RenkSeçici.ShowDialog() == DialogResult.Cancel)
                {
                    Görsel_Çıktı_ArkaPlanRenk.Checked = false;
                    return;
                }

                Ortak.ArkaPlanRengi = RenkSeçici.Color;
            }
            else
            {
                Ortak.ArkaPlanRengi = Color.Transparent;
            }

            Görsel_Çıktı_ArkaPlanRenk.FlatAppearance.CheckedBackColor = Ortak.ArkaPlanRengi;

            Çizdir();

            Kaydet.Enabled = true;
        }
        private void Görsel_Çıktı_Yakınlaştırma_Scroll(object sender, EventArgs e)
        {
            Ortak.YakınlaşmaOranı = Görsel_Çıktı_Yakınlaştırma.Value * 0.1f;
            Ortak.KullanılabilirAlan_piksel_Resim = new Size((int)(Ortak.KullanılabilirAlan_mm.Width * Ortak.YakınlaşmaOranı / 0.254), (int)(Ortak.KullanılabilirAlan_mm.Height * Ortak.YakınlaşmaOranı / 0.254));

            Çizdir();
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