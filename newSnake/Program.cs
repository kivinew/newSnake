using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace newSnake
{
    public sealed class Program : Form
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.Run(new Program());
        }

        //класс для удобства работы с координатами яблока и сегментов змеи
        public class Coord
        {
            public int X;
            public int Y;
            public Coord(int x, int y)
            {
                X = x; Y = y;
            }
        }

        Timer timer = new Timer();
        Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
        // условная ширина "поля действия" в клетках, высота и размер клетки в пикселях
        private const int W = 80;
        private const int H = 60;
        private const int S = 10;
        // собственно змея: список сегментов(нулевой индекс в списке - голова змеи)  
        List<Coord> snake = new List<Coord>();
        Coord _apple; // координаты яблока
        int _way; // направление движения змеи: 0 - вверх, 1 - вправо, 2 - вниз, 3 - влево
        int _apples; // количество собранных яблок
        int _stage = 1; // уровень игры
        int _score; // набранные очки в игре
        
        private Program()
        {
            Text = @"Snake"; // заголовок формы
            FormBorderStyle = FormBorderStyle.FixedDialog; // мышкой нельзя растягивать форму
            MaximizeBox = false; // делаем недоступной кнопку "развернуть во весь экран"
            StartPosition = FormStartPosition.CenterScreen; // форма отображается по центру экрана
            DoubleBuffered = true; // для прорисовки
            
            int captionSize = SystemInformation.CaptionHeight; // высота шапки формы
            int frameSize = SystemInformation.FrameBorderSize.Height; // ширина границы формы
            // устанавливаем размер внутренней области формы W * H с учетом высоты шапки и ширины границ
            Size = new Size(W * S + 2 * frameSize, H * S + captionSize + 2 * frameSize);

            Paint += Program_Paint; // привязываем обработчик прорисовки формы
            KeyDown += Program_KeyDown; // привязываем обработчик нажатий на кнопки

            timer.Interval = 200; // таймер срабатывает раз в 200 милисекунд
            timer.Tick += timer_Tick; // привязываем обработчик таймера
            timer.Start(); // запускаем таймер

            // делаем змею из трех сегментов, с начальными координатами внизу и по-центру формы
            snake.Add(new Coord(W / 2, H - 3));
            snake.Add(new Coord(W / 2, H - 2));
            snake.Add(new Coord(W / 2, H - 1));

            _apple = new Coord(rand.Next(W), rand.Next(H)); // координаты яблока
        }

        // обработка нажатий на клавиши(здесь только стрелки)
        // меняем направление движения, если оно не противоположное
        private void Program_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Up:
                    if (_way != 2)
                        _way = 0;
                    break;
                case Keys.Right:
                    if (_way != 3)
                        _way = 1;
                    break;
                case Keys.Down:
                    if (_way != 0)
                        _way = 2;
                    break;
                case Keys.Left:
                    if (_way != 1)
                        _way = 3;
                    break;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            // запоминаем координаты головы змеи
            int x = snake[0].X, y = snake[0].Y;
            // в зависимости от направления вычисляем где будет голова на следующем шаге
            // сделал чтобы при достижении края формы голова появлялась с противоположной стороны 
            // и змея продолжала движение
            switch (_way)
            {
                case 0:
                    y--;
                    if (y < 0)
                        y = H - 1;
                    break;
                case 1:
                    x++;
                    if (x >= W)
                        x = 0;
                    break;
                case 2:
                    y++;
                    if (y >= H)
                        y = 0;
                    break;
                case 3:
                    x--;
                    if (x < 0)
                        x = W - 1;
                    break;
            }
            var c = new Coord(x, y); // сегмент с новыми координатами головы
            snake.Insert(0, c); // вставляем его в начало списка сегментов змеи(змея выросла на один сегмент)
            if (snake[0].X == _apple.X && snake[0].Y == _apple.Y) // если координаты головы и яблока совпали
            {
                _apple = new Coord(rand.Next(W), rand.Next(H)); // располагаем яблоко в новых случайных координатах
                _apples++; // увеличиваем счетчик собранных яблок
                _score += _stage; // увеличиваем набранные очки в игре: за каждое яблоко прибавляем количество равное номеру уровня
                if (_apples % 10 == 0) // после каждого десятого яблока
                {
                    _stage++; // повышаем уровень
                    timer.Interval -= 10; // и уменьшаем интервал срабатывания яблока
                }
            }
            else // если координаты головы и яблока не совпали - убираем последний сегмент змеи(т.к. ранее добавляли новую голову)
                snake.RemoveAt(snake.Count - 1);
            Invalidate(); // перерисовываем, т.е. идет вызов Program_Paint
        }

        // собственно, отрисовка
        private void Program_Paint(object sender, PaintEventArgs e)
        {
            // рисуем красным кружком яблоко, синим квадратом голову змеи и зелеными квадратами тело змеи
            e.Graphics.FillEllipse(Brushes.Red, new Rectangle(_apple.X * S, _apple.Y * S, S, S));
            e.Graphics.FillRectangle(Brushes.Blue, new Rectangle(snake[0].X * S, snake[0].Y * S, S, S));
            for (int i = 1; i < snake.Count; i++)
                e.Graphics.FillRectangle(Brushes.Green, new Rectangle(snake[i].X * S, snake[i].Y * S, S, S));
            // сообщение о количестве собранных яблок, уровне и количестве очков
            string state = "Apples:" + _apples + "\n" +
                "Stage:" + _stage + "\n" + "Score:" + _score;
            // выводим это сообщение в левом верхнем углу
            e.Graphics.DrawString(state, new Font("Arial", 10, FontStyle.Italic), Brushes.Black, new Point(5, 5));
        }
    }
}