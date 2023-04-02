using ArgeMup.HazirKod;
using ArgeMup.HazirKod.Ekİşlemler;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;

namespace Etiket
{
    public static class Ortak
    {
        public static Depo_ Depo_Komut = null, Depo_Ayarlar = null;
        public static Görsel[] Görsel_ler = null;
        public static SizeF KullanılabilirAlan_mm;
        public static Size KullanılabilirAlan_piksel_Resim;
        public static Color ArkaPlanRengi = Color.Transparent;
        public static Görsel SeçiliGörsel = null;
        public static float YakınlaşmaOranı = 1;

        public static Color Renge(byte[] Girdi, Color BulunamamasıDurumundakiDeğeri)
        {
            if (Girdi == null || Girdi.Length != 4) return BulunamamasıDurumundakiDeğeri;
            return Color.FromArgb(Girdi[0], Girdi[1], Girdi[2], Girdi[3]);
        }
        public static string Renkten(Color Renk)
        {
            byte[] Girdi = new byte[4];
            Girdi[0] = Renk.A;
            Girdi[1] = Renk.R;
            Girdi[2] = Renk.G;
            Girdi[3] = Renk.B;

            return Girdi.HexYazıya();
        }

        public class Görsel
        {
            #region Değişkenler
            //Genel
            public string Adı;
            public RectangleF Çerçeve;
            public bool Yazı_1_Resim_0, Görünsün;
            public float Açı, EtKalınlığı;
            public string Yazıİçeriği_Veya_ResimDosyaYolu;
            public Color Renk, Renk_ArkaPlan;
            //Genel Yazı
            public string Yazı_KarakterKümesi;
            public int Yazı_Yaslama_Yatay, Yazı_Yaslama_Dikey;
            public bool Yazı_Kalın;
            //Genel Resim
            public float Resim_Yuvarlama_Çap;

            //İç Kullanım Yazı
            public Font Yazı_KarakterKümesi_;
            public SolidBrush Fırça_, Fırça_ArkaPlan_;
            public StringFormat Yazı_Şekli_;
            //İç Kullanım Resim
            public Image Resim_;
            public Pen Resim_Kalem_;
            public GraphicsPath Resim_Çerçeve_Yolu_;
            #endregion

            public Görsel(IDepo_Eleman Detaylar)
            {
                if (Detaylar == null) return;

                Adı = Detaylar.Adı;
                Yazı_1_Resim_0 = Detaylar.Oku_Bit(null, true, 0);
                Görünsün = Detaylar.Oku_Bit(null, true, 1);
                Açı = (float)Detaylar.Oku_Sayı(null, 0, 2);
                Renk = Renge(Detaylar.Oku_BaytDizisi(null, null, 3), Color.Black);
                Renk_ArkaPlan = Renge(Detaylar.Oku_BaytDizisi(null, null, 4), Color.Yellow);
                EtKalınlığı = (float)Detaylar.Oku_Sayı(null, Yazı_1_Resim_0 ? 0 : 1, 5);

                IDepo_Eleman k = Detaylar["Sol Üst Köşe"];
                Çerçeve = new RectangleF((float)k.Oku_Sayı(null, 0, 0), (float)k.Oku_Sayı(null, 0, 1), (float)k.Oku_Sayı(null, 10, 2), (float)k.Oku_Sayı(null, 5, 3));

                Yazıİçeriği_Veya_ResimDosyaYolu = Detaylar.Oku("İçerik", "Hatalı");

                k = Detaylar["Yazı"];
                Yazı_KarakterKümesi = k.Oku(null, "Calibri", 0);
                Yazı_Kalın = k.Oku_Bit(null, false, 1);
                Yazı_Yaslama_Yatay = k.Oku_TamSayı(null, 1, 2);
                Yazı_Yaslama_Dikey = k.Oku_TamSayı(null, 1, 3);

                k = Detaylar["Resim"];
                Resim_Yuvarlama_Çap = (float)k.Oku_Sayı(null, 2, 0);
            }
            public void Çizdir(Graphics Grafik, bool Seçildi)
            {
                if (Açı != 0)
                {
                    float _x = Çerçeve.X + (Çerçeve.Width / 2), _y = Çerçeve.Y + (Çerçeve.Height / 2);
                    Grafik.TranslateTransform(_x, _y); // döndürme noktası
                    Grafik.RotateTransform(Açı);
                    Grafik.TranslateTransform(-_x, -_y);
                }

                if (Yazı_1_Resim_0)
                {
                    if (Yazı_KarakterKümesi_ == null)
                    {
                        Fırça_ = new SolidBrush(Renk);
                        Fırça_ArkaPlan_ = new SolidBrush(Renk_ArkaPlan);
                        Yazı_Şekli_ = new StringFormat
                        {
                            LineAlignment = (StringAlignment)Yazı_Yaslama_Dikey,
                            Alignment = (StringAlignment)Yazı_Yaslama_Yatay
                        };

                        float KaBü_ = EtKalınlığı;
                        if (KaBü_ == 0)
                        {
                            if (string.IsNullOrEmpty(Yazıİçeriği_Veya_ResimDosyaYolu)) Yazıİçeriği_Veya_ResimDosyaYolu = " ";

                            SizeF Ölçü = new SizeF();
                            int KarakterSayısı = Yazıİçeriği_Veya_ResimDosyaYolu.Length, YazdırılanAdet = KarakterSayısı;

                            while (Ölçü.Width < Çerçeve.Width && Ölçü.Height < Çerçeve.Height && KarakterSayısı == YazdırılanAdet)
                            {
                                KaBü_ += 5;
                                Yazı_KarakterKümesi_ = new Font(Yazı_KarakterKümesi, KaBü_, Yazı_Kalın ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Millimeter);
                                Ölçü = Grafik.MeasureString(Yazıİçeriği_Veya_ResimDosyaYolu, Yazı_KarakterKümesi_, Çerçeve.Size, Yazı_Şekli_, out YazdırılanAdet, out _);
                                Yazı_KarakterKümesi_.Dispose();
                            }

                            while (Ölçü.Width >= Çerçeve.Width || Ölçü.Height >= Çerçeve.Height || KarakterSayısı != YazdırılanAdet)
                            {
                                KaBü_ -= 0.1f;
                                Yazı_KarakterKümesi_ = new Font(Yazı_KarakterKümesi, KaBü_, Yazı_Kalın ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Millimeter);
                                Ölçü = Grafik.MeasureString(Yazıİçeriği_Veya_ResimDosyaYolu, Yazı_KarakterKümesi_, Çerçeve.Size, Yazı_Şekli_, out YazdırılanAdet, out _);
                                Yazı_KarakterKümesi_.Dispose();
                            }
                        }

                        Yazı_KarakterKümesi_ = new Font(Yazı_KarakterKümesi, KaBü_, Yazı_Kalın ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Millimeter);
                    }

                    if (Renk_ArkaPlan != Color.Transparent) Grafik.FillRectangle(Fırça_ArkaPlan_, Çerçeve);

                    Grafik.DrawString(Yazıİçeriği_Veya_ResimDosyaYolu, Yazı_KarakterKümesi_, Fırça_, Çerçeve, Yazı_Şekli_);
                }
                else
                {
                    //Resimi hazırlama
                    if (Resim_ == null && File.Exists(Yazıİçeriği_Veya_ResimDosyaYolu))
                    {
                        Resim_ = Image.FromFile(Yazıİçeriği_Veya_ResimDosyaYolu);
                    }

                    if (Çerçeve.Width == 0)
                    {
                        if (Resim_ != null)
                        {
                            Grafik.PageUnit = GraphicsUnit.Pixel;
                            Grafik.DrawImageUnscaled(Resim_, (int)Çerçeve.Location.X, (int)Çerçeve.Location.Y);
                            Grafik.PageUnit = GraphicsUnit.Millimeter;
                        }
                    }
                    else
                    {
                        //Ön Hazırlık
                        if (EtKalınlığı > 0 || Renk_ArkaPlan != Color.Transparent)
                        {
                            if (Resim_Çerçeve_Yolu_ == null)
                            {
                                Resim_Çerçeve_Yolu_ = new GraphicsPath();
                                float EtKalınlığı_Bölü2 = EtKalınlığı / 2;
                                RectangleF çember = new RectangleF(Çerçeve.X + EtKalınlığı_Bölü2, Çerçeve.Y + EtKalınlığı_Bölü2, Çerçeve.Width - EtKalınlığı, Çerçeve.Height - EtKalınlığı);

                                if (Resim_Yuvarlama_Çap == 0)
                                {
                                    Resim_Çerçeve_Yolu_.AddRectangle(çember);
                                }
                                else
                                {
                                    RectangleF daire = new RectangleF(çember.Location, new SizeF(Resim_Yuvarlama_Çap, Resim_Yuvarlama_Çap));

                                    // sol üst
                                    Resim_Çerçeve_Yolu_.AddArc(daire, 180, 90);

                                    // sağ üst 
                                    daire.X = çember.Right - Resim_Yuvarlama_Çap;
                                    Resim_Çerçeve_Yolu_.AddArc(daire, 270, 90);

                                    // sağ alt
                                    daire.Y = çember.Bottom - Resim_Yuvarlama_Çap;
                                    Resim_Çerçeve_Yolu_.AddArc(daire, 0, 90);

                                    // sol alt
                                    daire.X = çember.Left;
                                    Resim_Çerçeve_Yolu_.AddArc(daire, 90, 90);
                                }

                                Resim_Çerçeve_Yolu_.CloseFigure(); 
                            }
                        }

                        //En alta resim arka plan boyama
                        if (Renk_ArkaPlan != Color.Transparent)
                        {
                            if (Fırça_ArkaPlan_ == null)
                            {
                                Fırça_ArkaPlan_ = new SolidBrush(Renk_ArkaPlan);
                            }

                            Grafik.FillPath(Fırça_ArkaPlan_, Resim_Çerçeve_Yolu_);
                        }

                        //Arkaplan üstüne resimi çizdirme
                        if (Resim_ != null)
                        {
                            Grafik.DrawImage(Resim_, Çerçeve);
                        }

                        //Resimin üzerine çerçeve çizdirme
                        if (EtKalınlığı > 0)
                        {
                            if (Fırça_ == null)
                            {
                                Fırça_ = new SolidBrush(Renk);
                                Resim_Kalem_ = new Pen(Fırça_, EtKalınlığı);
                            }

                            Grafik.DrawPath(Resim_Kalem_, Resim_Çerçeve_Yolu_);
                        }
                    }
                }

                if (Seçildi && Çerçeve.Width > 0)
                {
                    RectangleF r = Çerçeve;
                    Pen p_k = new Pen(Brushes.Red, 0.2f);
                    Pen p_b = new Pen(Brushes.White, 0.2f);

                    r.Inflate(0.1f, 0.1f);
                    Grafik.DrawRectangle(p_k, r.X, r.Y, r.Width, r.Height);

                    r.Inflate(0.2f, 0.2f);
                    Grafik.DrawRectangle(p_b, r.X, r.Y, r.Width, r.Height);

                    r.Inflate(0.2f, 0.2f);
                    Grafik.DrawRectangle(p_k, r.X, r.Y, r.Width, r.Height);

                    r.Inflate(0.2f, 0.2f);
                    Grafik.DrawRectangle(p_b, r.X, r.Y, r.Width, r.Height);

                    r.Inflate(0.2f, 0.2f);
                    Grafik.DrawRectangle(p_k, r.X, r.Y, r.Width, r.Height);
                }

                if (Açı != 0)
                {
                    Grafik.RotateTransform(-Açı);
                }

                Grafik.ResetTransform();
            }
            public void YenidenHesaplat()
            {
                Yazı_KarakterKümesi_?.Dispose(); Yazı_KarakterKümesi_ = null;
                Fırça_?.Dispose(); Fırça_ = null;
                Fırça_ArkaPlan_?.Dispose(); Fırça_ArkaPlan_ = null;
                Yazı_Şekli_?.Dispose(); Yazı_Şekli_ = null;

                Resim_?.Dispose(); Resim_ = null;
                Resim_Kalem_?.Dispose(); Resim_Kalem_ = null;
                Resim_Çerçeve_Yolu_?.Dispose(); Resim_Çerçeve_Yolu_ = null;
            }
            public void Depo_Kaydet(Depo_ Ayarlar)
            {
                IDepo_Eleman Detaylar = Ayarlar["Görseller/" + Adı];

                Detaylar.Adı = Adı;
                Detaylar.Yaz(null, Yazı_1_Resim_0, 0);
                Detaylar.Yaz(null, Görünsün, 1);
                Detaylar.Yaz(null, Açı, 2);
                Detaylar.Yaz(null, Renkten(Renk), 3);
                Detaylar.Yaz(null, Renkten(Renk_ArkaPlan), 4);
                Detaylar.Yaz(null, EtKalınlığı, 5);

                IDepo_Eleman a = Detaylar["Sol Üst Köşe"];
                a.Yaz(null, Çerçeve.X, 0);
                a.Yaz(null, Çerçeve.Y, 1);
                a.Yaz(null, Çerçeve.Width, 2);
                a.Yaz(null, Çerçeve.Height, 3);

                Detaylar.Yaz("İçerik", Yazıİçeriği_Veya_ResimDosyaYolu);

                Detaylar["Yazı"].İçeriği = new string[] { Yazı_KarakterKümesi, Yazı_Kalın.ToString(), Yazı_Yaslama_Yatay.ToString(), Yazı_Yaslama_Dikey.ToString() };

                Detaylar.Yaz("Resim", Resim_Yuvarlama_Çap);
            }
        }
        public static void Görseller_DizisiniOluştur(bool ÖnTanımlıGörselleriEkle, bool GüncelDeğerleriKullan, bool BoşluklarıEkle)
        {
            if (ÖnTanımlıGörselleriEkle)
            {
                IDepo_Eleman Tanımlılar = Depo_Komut["Ön Tanımlı Görseller"];
                foreach (IDepo_Eleman Tanımlı in Tanımlılar.Elemanları)
                {
                    IDepo_Eleman değişken = Depo_Ayarlar.Bul("Görseller/" + Tanımlı.Adı);
                    if (değişken == null)
                    {
                        Depo_Ayarlar.Yaz("Görseller/" + Tanımlı.Adı, Tanımlı[0]); //Yazı_1_Resim_0
                        Depo_Ayarlar.Yaz("Görseller/" + Tanımlı.Adı + "/İçerik", Tanımlı[1]); //İçerik
                    }
                }
            }

            if (GüncelDeğerleriKullan)
            {
                IDepo_Eleman Değişiklikler = Depo_Komut["Görsellerin Güncel Değerleri"];
                foreach (IDepo_Eleman Değişiklik in Değişiklikler.Elemanları)
                {
                    IDepo_Eleman değişken = Depo_Ayarlar.Bul("Görseller/" + Değişiklik.Adı);
                    if (değişken != null)
                    {
                        değişken["İçerik", 0] = Değişiklik[0];
                    }
                }
            }

            Görsel_ler = new Görsel[Depo_Ayarlar["Görseller"].Elemanları.Length];
            SizeF Boşluk = new SizeF((float)Depo_Ayarlar.Oku_Sayı("Yazıcı", 0, 1), (float)Depo_Ayarlar.Oku_Sayı("Yazıcı", 0, 2));
         
            for (int i = 0; i < Görsel_ler.Length; i++)
            {
                Görsel G = new Görsel(Depo_Ayarlar["Görseller"].Elemanları[i]);
                if (BoşluklarıEkle) G.Çerçeve.Location = PointF.Add(G.Çerçeve.Location, Boşluk); //yazıcı ölü alanının eklenmesi
                Görsel_ler[i] = G;
            }
        }
        public static Görsel Görseller_Görseli_Bul(string Adı)
        {
            for (int i = 0; i < Görsel_ler.Length; i++)
            {
                if (Görsel_ler[i].Adı == Adı) return Görsel_ler[i];
            }

            return null;
        }
        public static string Görseller_Görseli_Üret(Graphics Grafik)
        {
            string Hatalar = null;
            Grafik.ResetTransform();
            Grafik.PageUnit = GraphicsUnit.Millimeter;

            if (ArkaPlanRengi != Color.Transparent) Grafik.FillRectangle(new SolidBrush(ArkaPlanRengi), 0, 0, KullanılabilirAlan_mm.Width * YakınlaşmaOranı, KullanılabilirAlan_mm.Height * YakınlaşmaOranı);

            for (int i = Görsel_ler.Length - 1; i >= 0; i--)
            {
                if (!Görsel_ler[i].Görünsün || Görsel_ler[i] == SeçiliGörsel) continue;

                try
                {
                    if (YakınlaşmaOranı != 1) Grafik.ScaleTransform(YakınlaşmaOranı, YakınlaşmaOranı);
                    Görsel_ler[i].Çizdir(Grafik, false);
                }
                catch (Exception ex)
                {
                    Grafik.ResetTransform();
                    Hatalar += Görsel_ler[i].Adı + " -> " + ex.Message;
                }
            }

            //Seçilenin üstte görünmesi için en son çizdir
            if (SeçiliGörsel != null && SeçiliGörsel.Görünsün)
            {
                try
                {
                    if (YakınlaşmaOranı != 1) Grafik.ScaleTransform(YakınlaşmaOranı, YakınlaşmaOranı);
                    SeçiliGörsel.Çizdir(Grafik, true);
                }
                catch (Exception ex)
                {
                    Grafik.ResetTransform();
                    Hatalar += SeçiliGörsel.Adı + " -> " + ex.Message;
                }
            }

            return Hatalar;
        }
        public static string Görseller_Görseli_Yazdır()
        {
            string Hatalar = null;

            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = Depo_Ayarlar.Oku("Yazıcı", null, 0);
            pd.OriginAtMargins = false;
            pd.PrintPage += pd_Yazdır;
            if (pd.PrinterSettings.IsValid) pd.Print();
            else Hatalar += "Yazıcıya (" + pd.PrinterSettings.PrinterName + ") ulaşılamadı.";
            pd.Dispose();
            return Hatalar;

            void pd_Yazdır(object senderr, PrintPageEventArgs ev)
            {
                Hatalar += Görseller_Görseli_Üret(ev.Graphics);
            }
        }
        public static string Görseller_Görseli_ResimHalineGetir(out Image Resim)
        {
            Resim = new Bitmap(KullanılabilirAlan_piksel_Resim.Width, KullanılabilirAlan_piksel_Resim.Height);
            Graphics Grafik = Graphics.FromImage(Resim);
            string Hatalar = Görseller_Görseli_Üret(Grafik);
            Grafik.Dispose();
            return Hatalar;
        }
    }
}
