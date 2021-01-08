using GalaSoft.MvvmLight.Command;
using MemoramaPokemonWpf.Models;
using MemoramaPokemonWpf.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MemoramaPokemonWpf.ViewModels
{
    public enum Comando { Puntuacion, Iniciar }
    public class ImgPokemon : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void AlHaberCambios(string propiedad)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propiedad));
        }
        public int IdPokemon { get; set; }
        public string ImagenP { get; set; }
        private bool seleccion;

        public bool Seleccionado
        {
            get { return seleccion; }
            set
            {
                seleccion = value;
                AlHaberCambios("Seleccionado");
            }
        }
    }
    class PokemonViewModel : INotifyPropertyChanged
    {
        HttpClient client = new HttpClient() { BaseAddress = new Uri("https://pokeapi.co") };

        public ObservableCollection<Pokemon> Pokes = new ObservableCollection<Pokemon>();

        public event PropertyChangedEventHandler PropertyChanged;
        public byte Puntos { get; set; } = 0;
        public string Mensaje { get; set; }
        private bool puedeJugar;
        public bool PuedeJugar
        {
            get
            {
                return puedeJugar;
            }
            set
            {
                puedeJugar = value;
                AlHaberCambios("PuedeJugar");
            }
        }
        void AlHaberCambios(string propiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propiedad));
        }

        private ImgPokemon seleccion;

        public ImgPokemon Seleccionado
        {
            get { return seleccion; }
            set { seleccion = value;
                ValidacionCarta();
            }
        }
        Memorama memorama;
        public List<Pokemon> pokemones { get; set; } = new List<Pokemon>();
        public List<ImgPokemon> Historial { get; set; } = new List<ImgPokemon>();
        public List<ImgPokemon> Acertadas { get; set; } = new List<ImgPokemon>();
        public List<ImgPokemon> ListaCartaPokemon { get; set; } = new List<ImgPokemon>();
        public PokemonViewModel()
        {
            AsignarPokemones();
            GetPokemon();
            PuedeJugar = true;
        }
        public void AsignarPokemones()
        {
            byte[] carta = new byte[12] { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6 }; 
            var r = new Random();
            for (int i = 11; i > 0; i--)
            {
                var p = r.Next(0, 12);
                var c = carta[i];
                carta[i] = carta[p];
                carta[p] = c;
            }

            for (int i = 0; i < carta.Length; i++)
            {
                ImgPokemon cartas = new ImgPokemon
                {
                    ImagenP = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/" + carta[i] + ".png",
                    Seleccionado = false
                };
                ListaCartaPokemon.Add(cartas);
            }
        }
        public async void ValidacionCarta()
        {
            Seleccionado.Seleccionado = true;
            PuedeJugar = true;
            var Adivinada = 0;
            foreach (var item in Acertadas)
            {
                if (Seleccionado.ImagenP == item.ImagenP)
                {
                    Adivinada++;
                }
            }
            if (Adivinada < 1)
            {
                Historial.Add(Seleccionado);
                if (Historial.Count == 2)
                {
                    var count = 0;
                    ImgPokemon[] a = new ImgPokemon[2];
                    foreach (var item in Historial)
                    {
                        a[count] = item;
                        count++;
                    }
                    if (a[0].ImagenP == a[1].ImagenP)
                    {
                        Acertadas.Add(a[0]);
                        Acertadas.Add(a[1]);
                        CambiarMensaje("Cartas iguales");
                        Puntos++;
                        _ = JuegoGanado();
                    }
                    else
                    {
                        a[1].Seleccionado = true;
                        PuedeJugar = false;
                        await Task.Delay(1000);
                        PuedeJugar = true;
                        a[0].Seleccionado = false;
                        a[1].Seleccionado = false;
                        CambiarMensaje("Vuelve a intentar");
                    }
                    Historial.Clear();
                }
            }
            else
            {
                Historial.Clear();
                CambiarMensaje("Ya a sido adivinada esta carta");
            }
        }

        private void CambiarMensaje(string mensaje)
        {
            Mensaje = mensaje;
            Actualizar();
        }
        void Actualizar(string propertyName=null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        async Task JuegoGanado()
        {
            if (Puntos == 6)
            {
                PuedeJugar = false;
                await Task.Delay(1);
                CambiarMensaje("El juego a terminado. Encontro todos los pares");

            }
        }
        async void GetPokemon()
        {  
            Random r = new Random();
            int idP = r.Next(1, 893);
            var result = await client.GetAsync($"api/v2/pokemon/" + idP);
            if (result.IsSuccessStatusCode)
            {
                string datos = await result.Content.ReadAsStringAsync();
                var Pokemon = JsonConvert.DeserializeObject<Pokemon>(datos);
                Pokemon.imagen = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/" + idP + ".png";
                Pokes.Add(Pokemon);
                AlHaberCambios();
            }
        }
    }
}