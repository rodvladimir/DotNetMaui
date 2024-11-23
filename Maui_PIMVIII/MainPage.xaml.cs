using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;


namespace Maui_PIMVIII
{
    public class Song
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
    }
    public partial class MainPage : TabbedPage
    {
        private string _videoFilePath;
        private readonly HttpClient _httpClient;
        public ObservableCollection<Song> Playlist { get; set; }
        public ObservableCollection<Song> Playlist1 { get; set; }

        public bool VideoSelected { get; set; }

        public MainPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            Playlist = new ObservableCollection<Song>();

            Playlist1 = new ObservableCollection<Song>()
            {
                new Song { Name = "Música 1", Duration = TimeSpan.FromMinutes(3) },
                new Song { Name = "Música 2", Duration = TimeSpan.FromMinutes(5) },
                new Song { Name = "Música 3", Duration = TimeSpan.FromMinutes(4) }
            };
            BindingContext = this;
            UpdateMetrics();
        }

        private async void BTNClick_Clicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Selecione Conteúdo",
                FileTypes = FilePickerFileType.Videos
            });

            if (result != null)
            {
                _videoFilePath = result.FullPath;
                videoPathLabel.Text = _videoFilePath;
                VideoSelected = true;
                OnPropertyChanged(nameof(VideoSelected));
            }
        }

        private async void OnUploadVideoClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_videoFilePath))
                return;

            try
            {
                var content = new MultipartFormDataContent();
                var videoContent = new StreamContent(File.OpenRead(_videoFilePath));
                content.Add(videoContent, "file", Path.GetFileName(_videoFilePath));

                var response = await _httpClient.PostAsync("https://seu-servidor.com/upload", content);
                response.EnsureSuccessStatusCode();

                uploadStatusLabel.Text = "Upload concluído com sucesso!";
            }
            catch (Exception ex)
            {
                uploadStatusLabel.Text = $"Erro no upload: {ex.Message}";
            }
        }

        private void OnAddSongClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(songEntry.Text)) 
                { Playlist.Add(new Song { Name = songEntry.Text }); 
                songEntry.Text = string.Empty; 
            }
        }

        private void OnRemoveSongClicked(object sender, EventArgs e)
        {
            var button = (Button)sender; 
            var song = (Song)button.CommandParameter; 
            Playlist.Remove(song);
        }

        private void UpdateMetrics()
        {
            int totalSongs = Playlist1.Count;
            TimeSpan totalDuration = Playlist1.Aggregate(TimeSpan.Zero, (sum, song) => sum + song.Duration);
            TimeSpan averageDuration = TimeSpan.FromTicks(totalDuration.Ticks / (totalSongs > 0 ? totalSongs : 1));

            totalSongsLabel.Text = totalSongs.ToString();
            totalDurationLabel.Text = totalDuration.ToString(@"hh\:mm\:ss");
            averageDurationLabel.Text = averageDuration.ToString(@"mm\:ss");
        }
    }
}
