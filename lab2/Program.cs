using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LighthouseExpedition
{
    // ENUM’ы
    public enum ArtifactRarity { Common, Rare, Epic, Legendary }
    public enum ArtifactType { Relic, Tool, Map, Gem }

    // STRUCT (для координат артефакта)
    public struct Location
    {
        public double Latitude;
        public double Longitude;
        public int Depth;
        public override string ToString() => $"({Latitude:F2}, {Longitude:F2}, глубина: {Depth}м)";
    }

    // основной класс
    public class Artifact
    {
        public int Id { get; set; } // генерируется автоматически
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public ArtifactRarity Rarity { get; set; }
        public ArtifactType Type { get; set; }
        public Location Coordinates { get; set; }
        public double Weight { get; set; }
        public int Value { get; set; }
        public double Power { get; set; }
        public Explorer? Owner { get; set; }

        public override string ToString()
        {
            return $"[{Id}] {Name} | {Rarity}, {Type}, вес: {Weight}, сила: {Power}, ценность: {Value} | {Coordinates}";
        }
    }

    public class Explorer
    {
        public string Name { get; set; }
        public string Rank { get; set; }
        public int Experience { get; set; }
        public override string ToString() => $"{Name} ({Rank}, опыт: {Experience})";
    }

    public class ArtifactManager
    {
        private LinkedList<Artifact> _artifacts = new();
        private int _nextId = 1;
        private readonly string _filePath;

        public ArtifactManager(string filePath)
        {
            _filePath = filePath;
            Load();
        }

        // Загрузка из JSON
        private void Load()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    var json = File.ReadAllText(_filePath);
                    var data = JsonSerializer.Deserialize<List<Artifact>>(json);
                    if (data != null)
                    {
                        _artifacts = new LinkedList<Artifact>(data);
                        _nextId = _artifacts.Any() ? _artifacts.Max(a => a.Id) + 1 : 1;
                    }
                }
                catch
                {
                    Console.WriteLine("Ошибка чтения файла, коллекция пуста.");
                }
            }
        }

        // Сохранение в JSON
        public void Save()
        {
            var json = JsonSerializer.Serialize(_artifacts.ToList(), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        // Команды
        public void Info()
        {
            Console.WriteLine($"Тип коллекции: {nameof(LinkedList<Artifact>)}");
            Console.WriteLine($"Дата инициализации: {DateTime.Now}");
            Console.WriteLine($"Количество элементов: {_artifacts.Count}");
        }

        public void Show()
        {
            if (_artifacts.Count == 0)
            {
                Console.WriteLine("Коллекция пуста.");
                return;
            }
            foreach (var art in _artifacts)
                Console.WriteLine(art);
        }

        public void Insert(Artifact artifact)
        {
            artifact.Id = _nextId++;
            _artifacts.AddLast(artifact);
            Console.WriteLine("✅ Артефакт добавлен.");
        }

        public void RemoveById(int id)
        {
            var node = _artifacts.FirstOrDefault(a => a.Id == id);
            if (node == null) Console.WriteLine("Не найдено.");
            else
            {
                _artifacts.Remove(node);
                Console.WriteLine("❌ Удалено.");
            }
        }

        public void Clear()
        {
            _artifacts.Clear();
            Console.WriteLine("Коллекция очищена.");
        }

        // Доп. команды по варианту
        public void ReplaceIfLower(int id, Artifact newArtifact)
        {
            var old = _artifacts.FirstOrDefault(a => a.Id == id);
            if (old == null)
            {
                Console.WriteLine("Элемент не найден.");
                return;
            }

            if (newArtifact.Value < old.Value)
            {
                newArtifact.Id = id;
                var node = _artifacts.Find(old);
                _artifacts.AddBefore(node!, newArtifact);
                _artifacts.Remove(node!);
                Console.WriteLine("Заменён, так как новое значение меньше.");
            }
            else Console.WriteLine("Новое значение не меньше, замена не выполнена.");
        }

        public void FilterByRarity(string rarityStr)
        {
            if (Enum.TryParse<ArtifactRarity>(rarityStr, true, out var rarity))
            {
                var filtered = _artifacts.Where(a => a.Rarity == rarity);
                foreach (var a in filtered)
                    Console.WriteLine(a);
            }
            else Console.WriteLine("Неверное значение редкости.");
        }

        public void GroupByType()
        {
            var groups = _artifacts.GroupBy(a => a.Type);
            foreach (var g in groups)
            {
                Console.WriteLine($"\nТип: {g.Key}");
                foreach (var a in g)
                    Console.WriteLine($"  - {a.Name}");
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            string file = args.Length > 0 ? args[0] : "artifacts.json";
            var manager = new ArtifactManager(file);

            Console.WriteLine("Добро пожаловать в систему управления артефактами!");
            Console.WriteLine("Введите 'help' для списка команд.\n");

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts[0].ToLower();
                var arg = parts.Length > 1 ? parts[1] : "";

                try
                {
                    switch (cmd)
                    {
                        case "help":
                            Console.WriteLine("Команды: info, show, insert, remove_key <id>, clear, save, replace_if_lower <id>, filter_rarity <name>, group_by_type, exit");
                            break;
                        case "info": manager.Info(); break;
                        case "show": manager.Show(); break;
                        case "clear": manager.Clear(); break;
                        case "save": manager.Save(); Console.WriteLine("✅ Сохранено."); break;
                        case "remove_key":
                            if (int.TryParse(arg, out var idr)) manager.RemoveById(idr);
                            else Console.WriteLine("Укажите id.");
                            break;
                        case "replace_if_lower":
                            if (int.TryParse(arg, out var rid))
                                manager.ReplaceIfLower(rid, CreateArtifact());
                            else Console.WriteLine("Неверный id.");
                            break;
                        case "filter_rarity":
                            manager.FilterByRarity(arg);
                            break;
                        case "group_by_type":
                            manager.GroupByType();
                            break;
                        case "insert":
                            manager.Insert(CreateArtifact());
                            break;
                        case "exit":
                            manager.Save();
                            return;
                        default:
                            Console.WriteLine("Неизвестная команда. help — список команд.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        // Ввод нового артефакта
        static Artifact CreateArtifact()
        {
            var art = new Artifact();

            Console.Write("Название: ");
            art.Name = Console.ReadLine() ?? "";

            Console.Write("Описание: ");
            art.Description = Console.ReadLine() ?? "";

            Console.Write("Редкость (Common, Rare, Epic, Legendary): ");
            Enum.TryParse(Console.ReadLine(), true, out ArtifactRarity rarity);
            art.Rarity = rarity;

            Console.Write("Тип (Relic, Tool, Map, Gem): ");
            Enum.TryParse(Console.ReadLine(), true, out ArtifactType type);
            art.Type = type;
            Location loc = new Location();

            Console.Write("Широта: ");
            loc.Latitude = double.Parse(Console.ReadLine() ?? "0");

            Console.Write("Долгота: ");
            loc.Longitude = double.Parse(Console.ReadLine() ?? "0");

            Console.Write("Глубина: ");
            loc.Depth = int.Parse(Console.ReadLine() ?? "0");

            art.Coordinates = loc;

            Console.Write("Вес: ");
            art.Weight = double.Parse(Console.ReadLine() ?? "0");

            Console.Write("Ценность: ");
            art.Value = int.Parse(Console.ReadLine() ?? "0");

            Console.Write("Сила: ");
            art.Power = double.Parse(Console.ReadLine() ?? "0");

            Console.Write("Владелец (имя): ");
            var ownerName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(ownerName))
                art.Owner = new Explorer { Name = ownerName, Rank = "Новичок", Experience = 0 };

            return art;
        }
    }
}
