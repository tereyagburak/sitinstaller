using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;

// evet amk GPT kullandım hemen GPT KULLANMIŞ HÜÜ diye zırlamayın. gpt kullanmayan kaldı mı sizce.

namespace SitInstaller
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Kullanıcıdan video linkini alıyoruz
            Console.Write("Video linkini girin. https://sitwatch.net/watch/456 bir örnektir: ");
            string url = Console.ReadLine();

            // Eğer URL boşsa, varsayılan bir URL kullanıyoruz
            if (string.IsNullOrEmpty(url))
            {
                url = "https://sitwatch.net/watch/456";
            }

            // Video linkini alıyoruz
            string videoLink = GetVideoLink(url);

            // Video linkini ekrana yazdırıyoruz
            Console.WriteLine($"Video Bağlantısı: {videoLink}");

            // Video bağlantısı varsa, indirme işlemini başlatıyoruz
            if (!string.IsNullOrEmpty(videoLink))
            {
                DownloadVideo(videoLink);
            }

            // Kullanıcı çıkmak için bir tuşa basar
            Console.WriteLine("Çıkmak için bir tuşa basın.");
            Console.ReadKey();
        }

        // Video linkini almak için metod
        static string GetVideoLink(string url)
        {
            HttpClient client = new HttpClient();

            try
            {
                // Sitenin HTML içeriğini indiriyoruz
                string htmlContent = client.GetStringAsync(url).Result;

                // <video> etiketindeki data-video-url veya src özniteliklerinden video bağlantısını ayıklamak için düzenli ifadeler
                Regex videoUrlRegex = new Regex(@"<video[^>]*data-video-url=[""'](?<dataVideoUrl>[^""']+)[""']", RegexOptions.IgnoreCase);
                Regex videoSrcRegex = new Regex(@"<video[^>]*src=[""'](?<src>[^""']+)[""']", RegexOptions.IgnoreCase);

                // İlk olarak data-video-url'yi kontrol ediyoruz
                Match match = videoUrlRegex.Match(htmlContent);
                if (!match.Success)
                {
                    // Eğer data-video-url bulunmazsa src özniteliğini kontrol ediyoruz
                    match = videoSrcRegex.Match(htmlContent);
                }

                if (match.Success)
                {
                    // Video bağlantısı varsa alıyoruz
                    string videoLink = match.Groups[1].Value;
                    return videoLink.StartsWith("/") ? $"https://sitwatch.net{videoLink}" : videoLink; // Eğer bağlantı kök yol içeriyorsa, tam URL'yi oluşturuyoruz
                }
                else
                {
                    return "Video bağlantısı bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                return $"Hata: {ex.Message}";
            }
        }

        // Video indirme işlemi için metod
        static void DownloadVideo(string url)
        {
            Console.WriteLine("Video indirme işlemi 4 saniye içinde başlayacak. İptal etmek istiyorsanız tam şu an programı kapatın.");
            System.Threading.Thread.Sleep(4000);

            HttpClient client = new HttpClient();

            // Klasör yolunu belgeler klasörüne göre belirliyoruz
            string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SitInstalled");

            // Klasör yoksa oluşturuyoruz
            if (!Directory.Exists(downloadsFolder))
            {
                try
                {
                    Directory.CreateDirectory(downloadsFolder);
                    Console.WriteLine("Klasör oluşturuldu: " + downloadsFolder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Klasör oluşturulurken hata oluştu: " + ex.Message);
                    return; // Hata oluşursa, işlemi sonlandırıyoruz
                }
            }

            try
            {
                Console.WriteLine($"İndirme işlemi başladı. Bitince size haber vereceğiz.");

                // Dosya içeriğini alıyoruz
                byte[] fileBytes = client.GetByteArrayAsync(url).Result;

                // URL'den dosya adını alıyoruz
                string fileName = Path.GetFileName(url);
                string filePath = Path.Combine(downloadsFolder, fileName);

                // Dosyayı kaydediyoruz
                File.WriteAllBytes(filePath, fileBytes);

                Console.WriteLine("Dosya başarıyla indirildi: " + filePath);
                Console.WriteLine("Dosyayı açmak ister misiniz? (E/H)");

                // Dosyayı açmak için kullanıcıdan onay alıyoruz
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.E)
                {
                    // Dosyayı varsayılan uygulama ile açıyoruz
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,  // Dosya yolunu belirtiyoruz
                        UseShellExecute = true // Varsayılan uygulama ile açılması için true yapıyoruz
                    });

                    Console.WriteLine("SitInstaller kullandığınız için teşekkürler!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dosya indirilirken hata oluştu: {ex.Message}");
            }
        }
    }
}
