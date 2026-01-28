using RogueLike_PROJECT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RogueLike_PROJECT
{
    public class GameForm : Form
    {

        private readonly Panel _menuPanel = new();
        private readonly Panel _controlsPanel = new();
        private readonly Panel _confirmPanel = new();
        private Panel _confirmCard = new();


        private readonly Button _btnNewGame = new() { Text = "Новая игра", Width = 220, Height = 48 };
        private readonly Button _btnControls = new() { Text = "Управление", Width = 220, Height = 48 };
        private readonly Button _btnExit = new() { Text = "Выйти", Width = 220, Height = 48 };


        private readonly Button _btnBackFromControls = new() { Text = "Назад", Width = 220, Height = 48 };
        private readonly Label _controlsLabel = new()
        {
            AutoSize = true,
            Text =
                "Управление:\n" +
                "WASD — ходьба\n" +
                "Мышь — направление взгляда\n" +
                "ЛКМ — атака\n" +
                "Space — перекат (неуязвимость)\n" +
                "ESC — выход в меню "
        };


        private readonly Label _confirmLabel = new() { AutoSize = true, Text = "Выйти в главное меню? (прогресс уровня потеряется)" };
        private readonly Button _btnYes = new() { Text = "Да", Width = 140, Height = 44 };
        private readonly Button _btnNo = new() { Text = "Нет", Width = 140, Height = 44 };


        private GameState _state = GameState.MainMenu;

        private readonly Stopwatch _sw = Stopwatch.StartNew();
        private long _lastTicks;
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 16 };

        private GameWorld? _world;

        private void InitializeComponent()
        {

        }

        public GameForm()
        {
            Text = "RogueLike PROJECT";
            ClientSize = new Size(1920, 1080);
            StartPosition = FormStartPosition.CenterScreen;

            DoubleBuffered = true;
            KeyPreview = true;

            AcceptButton = null;
            CancelButton = null;

            BuildMenuUI();
            BuildControlsUI();
            BuildConfirmUI();

            ShowMenu();

            _timer.Tick += (_, __) => TickGame();
            _timer.Start();

            KeyDown += OnKeyDown;
            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
        }
        private Panel _menuCard = new();

        private void BuildMenuUI()
        {
            _menuPanel.Dock = DockStyle.Fill;
            _menuPanel.BackColor = Color.FromArgb(18, 18, 22);

            // Карточка
            _menuCard = new Panel
            {
                Size = new Size(520, 420),
                BackColor = Color.FromArgb(30, 30, 36),
                BorderStyle = BorderStyle.FixedSingle
            };

            var title = new Label
            {
                Dock = DockStyle.Top,
                Height = 120,
                Text = "ROGUELIKE PROJECT",
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 28, FontStyle.Bold)
            };

            // Кнопки — по центру
            SetupMenuButton(_btnNewGame);
            SetupMenuButton(_btnControls);
            SetupMenuButton(_btnExit);

            var flow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.None
            };
            flow.Controls.Add(_btnNewGame);
            flow.Controls.Add(Spacer(14));
            flow.Controls.Add(_btnControls);
            flow.Controls.Add(Spacer(14));
            flow.Controls.Add(_btnExit);

            // Контейнер для вертикального центрирования внутри карточки
            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            inner.Controls.Add(flow, 0, 1);

            _menuCard.Controls.Add(inner);
            _menuCard.Controls.Add(title);

            // Центрирование карточки на экране
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            root.Controls.Add(_menuCard, 1, 1);

            _menuPanel.Controls.Add(root);
            Controls.Add(_menuPanel);

            // Обработчики
            _btnNewGame.Click += (_, __) => StartNewGame();
            _btnControls.Click += (_, __) => ShowControls();
            _btnExit.Click += (_, __) => Close();
        }

        private void SetupMenuButton(Button b)
        {
            b.Width = 360;
            b.Height = 56;
            b.Font = new System.Drawing.Font("Segoe UI", 16, FontStyle.Bold);
            b.FlatStyle = FlatStyle.Flat;
            b.ForeColor = Color.White;
            b.BackColor = Color.FromArgb(45, 45, 55);
            b.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 95);
            b.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void CenterMenuCard()
        {
            _menuCard.Left = (ClientSize.Width - _menuCard.Width) / 2;
            _menuCard.Top = (ClientSize.Height - _menuCard.Height) / 2;
        }

        private Panel _controlsCard = new();

        private void BuildControlsUI()
        {
            _controlsPanel.Dock = DockStyle.Fill;
            _controlsPanel.BackColor = Color.FromArgb(18, 18, 22);

            _controlsCard = new Panel
            {
                Size = new Size(720, 520),
                BackColor = Color.FromArgb(30, 30, 36),
                BorderStyle = BorderStyle.FixedSingle
            };

            var title = new Label
            {
                Dock = DockStyle.Top,
                Height = 110,
                Text = "УПРАВЛЕНИЕ",
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 24, FontStyle.Bold)
            };

            _controlsLabel.ForeColor = Color.White;
            _controlsLabel.Font = new System.Drawing.Font("Segoe UI", 14, FontStyle.Regular);

            SetupMenuButton(_btnBackFromControls);
            _btnBackFromControls.Text = "НАЗАД";

            var flow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Dock = DockStyle.Fill,
                Padding = new Padding(40),
            };
            flow.Controls.Add(_controlsLabel);
            flow.Controls.Add(Spacer(24));
            flow.Controls.Add(_btnBackFromControls);

            _btnBackFromControls.Click += (_, __) => ShowMenu();

            _controlsCard.Controls.Add(flow);
            _controlsCard.Controls.Add(title);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            root.Controls.Add(_controlsCard, 1, 1);

            _controlsPanel.Controls.Add(root);
            Controls.Add(_controlsPanel);
        }
        private void CenterConfirmCard()
        {
            if (_confirmCard == null) return;
            _confirmCard.Left = (ClientSize.Width - _confirmCard.Width) / 2;
            _confirmCard.Top = (ClientSize.Height - _confirmCard.Height) / 2;
        }

        private void BuildConfirmUI()
        {
            _confirmPanel.Dock = DockStyle.Fill;
            _confirmPanel.BackColor = Color.FromArgb(180, 0, 0, 0);
            _confirmPanel.Visible = false;
            _confirmPanel.Enabled = false;

            _confirmCard = new Panel
            {
                Size = new Size(520, 180),
                BackColor = Color.FromArgb(240, 30, 30, 30),
            };
            _confirmPanel.Controls.Add(_confirmCard);
            Controls.Add(_confirmPanel);

            CenterConfirmCard();
            Resize += (_, __) => CenterConfirmCard();

            _confirmLabel.ForeColor = Color.White;

            var btnRow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true
            };
            btnRow.Controls.Add(_btnYes);
            btnRow.Controls.Add(_btnNo);

            var flow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                WrapContents = false,
                Padding = new Padding(18),
            };
            flow.Controls.Add(_confirmLabel);
            flow.Controls.Add(Spacer(16));
            flow.Controls.Add(btnRow);

            _btnYes.Click += (_, __) => { _world = null; ShowMenu(); };
            _btnNo.Click += (_, __) => { _state = GameState.Playing; _confirmPanel.Visible = false; };

            _confirmCard.Controls.Add(flow);
            _confirmPanel.Visible = false;

            Controls.Add(_confirmPanel);
        }

        private Control Spacer(int h) => new Panel { Height = h, Width = 10 };

        private void ShowMenu()
        {
            _state = GameState.MainMenu;
            _menuPanel.Visible = true;
            _controlsPanel.Visible = false;
            _confirmPanel.Visible = false;
            _confirmPanel.Enabled = false;
            ActiveControl = null;
            _confirmPanel.Visible = false;
            _confirmPanel.Enabled = false;
            Focus();
            Invalidate();
        }

        private void ShowControls()
        {
            _state = GameState.Controls;
            _menuPanel.Visible = false;
            _controlsPanel.Visible = true;
            _confirmPanel.Visible = false;
            _confirmPanel.Enabled = false;
            ActiveControl = null;
            _confirmPanel.Visible = false;
            _confirmPanel.Enabled = false;
            Focus();
            Invalidate();
        }

        private void StartNewGame()
        {
            _state = GameState.Playing;
            _menuPanel.Visible = false;
            _controlsPanel.Visible = false;
            _confirmPanel.Visible = false;

            _world = GameWorld.FromMap(Maps.PickRandom());
            ActiveControl = null;
            Focus();
            Invalidate();
        }

        private void TickGame()
        {
            var now = _sw.ElapsedMilliseconds;
            var dt = (now - _lastTicks) / 1000f;
            _lastTicks = now;

            if (_state == GameState.Playing && _world != null)
            {
                _world.Update(dt);
                Invalidate(); // перерисовка
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_state == GameState.Playing && _world != null)
            {
                _world.Draw(e.Graphics, ClientSize);
            }
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (_state == GameState.Playing && _world != null)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                if (e.KeyCode == Keys.Escape)
                {
                    _state = GameState.ConfirmExitToMenu;
                    _confirmPanel.Enabled = true;
                    _confirmPanel.Visible = true;
                    _confirmPanel.BringToFront();
                    CenterConfirmCard();
                    ActiveControl = null;
                    Focus();
                    return;
                }
                _world.OnKeyPressed(e.KeyCode);
                _world.Input.SetKey(e.KeyCode, true);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (_state == GameState.Playing && _world != null)
            {
                _world.Input.SetKey(e.KeyCode, false);
            }
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (_state == GameState.Playing && _world != null)
            {
                if (e.Button == MouseButtons.Left)
                    _world.Input.MouseLeftDown = true;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_state == GameState.Playing && _world != null)
            {
                if (e.Button == MouseButtons.Left)
                    _world.Input.MouseLeftDown = false;
            }
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (_state == GameState.Playing && _world != null)
            {
                _world.Input.MousePos = new Vector2(e.X, e.Y);
            }
        }
    }

    // GAME WORLD ==================================================================================

    public sealed class InputState
    {
        public bool W, A, S, D, Space;
        public bool D1, D2, D3, D4;

        public bool MouseLeftDown;
        public Vector2 MousePos;

        private bool _prevMouseLeftDown;
        public bool MouseLeftPressedThisFrame => MouseLeftDown && !_prevMouseLeftDown;

        public void EndFrame() => _prevMouseLeftDown = MouseLeftDown;

        public void SetKey(Keys k, bool down)
        {
            if (k == Keys.W) W = down;
            if (k == Keys.A) A = down;
            if (k == Keys.S) S = down;
            if (k == Keys.D) D = down;
            if (k == Keys.Space) Space = down;

            if (k == Keys.D1) D1 = down;
            if (k == Keys.D2) D2 = down;
            if (k == Keys.D3) D3 = down;
            if (k == Keys.D4) D4 = down;
        }
    }

    // ---------- STATS / ENTITIES ----------
    public struct Stats
    {
        public float MaxHp;
        public float Hp;
        public float Armor;
        public float Speed; // px/sec
        public bool IsDead => Hp <= 0;
    }

    public abstract class Entity
    {
        public Vector2 Pos;
        public float Radius;
        public Stats Stats;

        public bool IsDead => Stats.IsDead;

        public void TakeDamage(float rawDamage)
        {
            float dealt = MathF.Max(1f, rawDamage - Stats.Armor);
            Stats.Hp = MathF.Max(0, Stats.Hp - dealt);
        }
    }

    // ---------- WEAPONS ----------
    public enum WeaponType { Sword = 1, Axe = 2, Spear = 3, Bow = 4 }

    public readonly struct WeaponData
    {
        public readonly string Name;
        public readonly float Damage;
        public readonly float AttackSpeed;   // attacks/sec
        public readonly float Range;         // melee range OR spawn distance for bow
        public readonly float ConeDot;       // melee cone
        public readonly float ProjectileSpeed;

        public WeaponData(string name, float dmg, float atkSpd, float range, float coneDot, float projSpeed)
        {
            Name = name;
            Damage = dmg;
            AttackSpeed = atkSpd;
            Range = range;
            ConeDot = coneDot;
            ProjectileSpeed = projSpeed;
        }

        public float CooldownSeconds => AttackSpeed <= 0 ? 999f : (1f / AttackSpeed);
        public bool IsRanged => ProjectileSpeed > 0;
    }

    public static class WeaponDB
    {
        public static WeaponData Get(WeaponType w) => w switch
        {
            WeaponType.Sword => new WeaponData("Меч", dmg: 22, atkSpd: 2.2f, range: 85, coneDot: 0.25f, projSpeed: 0),
            WeaponType.Axe => new WeaponData("Топор", dmg: 32, atkSpd: 1.4f, range: 75, coneDot: 0.15f, projSpeed: 0),
            WeaponType.Spear => new WeaponData("Копьё", dmg: 24, atkSpd: 1.8f, range: 105, coneDot: 0.55f, projSpeed: 0),
            WeaponType.Bow => new WeaponData("Лук", dmg: 18, atkSpd: 2.0f, range: 30, coneDot: 0.0f, projSpeed: 520),
            _ => new WeaponData("?", 10, 1, 60, 0.25f, 0)
        };
    }

    // ---------- ITEMS / INVENTORY ----------
    public enum ItemSlot { Weapon, Armor, Accessory }
    public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

    public sealed class Item
    {
        public string Name = "";
        public ItemSlot Slot;
        public ItemRarity Rarity;

        public WeaponType WeaponType;

        public float AddDamage;
        public float AddArmor;
        public float AddSpeed;
        public float AddAttackSpeed;
        public float AddRange;
        public float AddMaxHp;

        public float Score()
        {
            float baseScore = AddDamage * 2f + AddArmor * 1.5f + AddMaxHp * 0.4f + AddSpeed * 0.2f + AddAttackSpeed * 3f + AddRange * 0.15f;
            return baseScore * RarityMult(Rarity);
        }
        public static float RarityMult(ItemRarity r) => r switch
        {
            ItemRarity.Common => 1.0f,
            ItemRarity.Uncommon => 1.25f,
            ItemRarity.Rare => 1.6f,
            ItemRarity.Epic => 2.1f,
            ItemRarity.Legendary => 2.8f,
            _ => 1f
        };
        public string RarityTag => Rarity switch
        {
            ItemRarity.Common => "Обычный",
            ItemRarity.Uncommon => "Необычный",
            ItemRarity.Rare => "Редкий",
            ItemRarity.Epic => "Эпический",
            ItemRarity.Legendary => "Легендарный",
            _ => "?"
        };
    }

    public sealed class Inventory
    {
        public readonly List<Item> Items = new();

        public Item? EquippedWeaponItem;
        public Item? EquippedArmor;
        public Item? EquippedAccessory;

        public void Add(Item it) => Items.Add(it);

        public void Equip(Item it, Player p)
        {
            if (it.Slot == ItemSlot.Weapon)
            {
                // Снимаем прошлое оружие в инвентарь
                if (EquippedWeaponItem != null) Items.Add(EquippedWeaponItem);
                EquippedWeaponItem = it;
                p.EquippedWeapon = it.WeaponType; // тип оружия меняем на тип предмета
            }
            else if (it.Slot == ItemSlot.Armor)
            {
                if (EquippedArmor != null) Items.Add(EquippedArmor);
                EquippedArmor = it;
            }
            else
            {
                if (EquippedAccessory != null) Items.Add(EquippedAccessory);
                EquippedAccessory = it;
            }

            Items.Remove(it);
        }

        public IEnumerable<Item> EquippedAll()
        {
            if (EquippedWeaponItem != null) yield return EquippedWeaponItem;
            if (EquippedArmor != null) yield return EquippedArmor;
            if (EquippedAccessory != null) yield return EquippedAccessory;
        }
    }

    // ---------- PLAYER / ENEMY ----------
    public sealed class Player : Entity
    {
        public float AimAngleRad;
        public float AttackCd;
        public float InvulnTimer;
        public Animator Anim = new Animator();
        public Dir4 Dir = Dir4.Down;
        public float AttackAnimLock;
        public float RollTimer;
        public Dir4 RollDir = Dir4.Down;
        public Vector2 RollMoveDir = Vector2.Zero;


        public float SlowTimer;
        public float SlowFactor = 1f;

        public WeaponType EquippedWeapon = WeaponType.Sword;
        public readonly Inventory Inv = new();

        // База (без предметов)
        public readonly Stats BaseStats;

        public Player(Vector2 startPos)
        {
            Pos = startPos;
            Radius = 16f;

            BaseStats = new Stats
            {
                MaxHp = 100,
                Hp = 100,
                Armor = 2,
                Speed = 150
            };

            Stats = BaseStats;
        }

        public void RecomputeStats()
        {
            float maxHp = BaseStats.MaxHp;
            float armor = BaseStats.Armor;
            float speed = BaseStats.Speed;

            foreach (var it in Inv.EquippedAll())
            {
                maxHp += it.AddMaxHp;
                armor += it.AddArmor;
                speed += it.AddSpeed;
            }

            // сохраняем текущие HP, но учитываем новый максимум
            float hp = MathF.Min(Stats.Hp, maxHp);
            Stats.MaxHp = maxHp;
            Stats.Armor = armor;
            Stats.Speed = speed;
            Stats.Hp = hp;
        }

        public WeaponData GetWeaponData()
        {
            var w = WeaponDB.Get(EquippedWeapon);

            float dmg = w.Damage;
            float atkSpd = w.AttackSpeed;
            float range = w.Range;

            foreach (var it in Inv.EquippedAll())
            {
                dmg += it.AddDamage;
                atkSpd += it.AddAttackSpeed;
                range += it.AddRange;
            }

            atkSpd = MathF.Max(0.2f, atkSpd);

            return new WeaponData(w.Name, dmg, atkSpd, range, w.ConeDot, w.ProjectileSpeed);
        }
    }

    public enum EnemyType { Zombie, SkeletonArcher, Spider }

    public sealed class Enemy : Entity
    {
        public EnemyType Type;

        public float Damage;
        public float AttackSpeed;     
        public float AttackRange;     
        public float ProjectileSpeed; 
        public float PreferredRange;
        public Animator Anim = new Animator();
        public Dir4 Dir = Dir4.Down;
        
        public float AttackCd;
        public float AimAngleRad;

        public bool IsRanged => ProjectileSpeed > 0;
        public float AttackCooldownSeconds => AttackSpeed <= 0 ? 999f : (1f / AttackSpeed);

        public Enemy(EnemyType type, Vector2 pos)
        {
            Type = type;
            Pos = pos;

            switch (type)
            {
                case EnemyType.Zombie:
                    Radius = 16f;
                    Stats = new Stats { MaxHp = 50, Hp = 50, Armor = 1, Speed = 120 };
                    Damage = 10f; AttackSpeed = 1.2f; AttackRange = 55f;
                    ProjectileSpeed = 0f; PreferredRange = 0f;
                    break;

                case EnemyType.SkeletonArcher:
                    Radius = 15f;
                    Stats = new Stats { MaxHp = 35, Hp = 35, Armor = 0f, Speed = 110 };
                    Damage = 9f; AttackSpeed = 1.1f; AttackRange = 360f;
                    ProjectileSpeed = 460f; PreferredRange = 220f;
                    break;

                case EnemyType.Spider:
                    Radius = 15f;
                    Stats = new Stats { MaxHp = 40, Hp = 40, Armor = 0.5f, Speed = 135 };
                    Damage = 7f; AttackSpeed = 1.4f; AttackRange = 55f;
                    ProjectileSpeed = 380f; PreferredRange = 140f;
                    break;
            }
        }
    }

    // Босс
    public enum BossType { GraveBrute, BoneLord, WebQueen }

    public sealed class Boss : Entity
    {
        public BossType Type;
        public float MeleeCd;
        public float SpecialCd;
        public float Windup;     // для “удар по земле”
        public Vector2 StoredPos; // позиция для удара

        public float MeleeDamage;
        public float MeleeRange;
        public float MoveSpeed;

        public Boss(BossType type, Vector2 pos)
        {
            Type = type;
            Pos = pos;
            Radius = 52f;

            Stats = new Stats { MaxHp = 320, Hp = 320, Armor = 3.5f, Speed = 0 };
            MoveSpeed = 95f;

            MeleeDamage = 16f;
            MeleeRange = 70f;

            SpecialCd = 2.0f;
        }
    }

    // ---------- PROJECTILES ----------
    public sealed class Projectile
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public float Radius;
        public float Damage;
        public float Life;
        public bool FromPlayer;

        public float SlowSeconds;
        public float SlowFactor;

        public Projectile(Vector2 pos, Vector2 vel, float radius, float damage, float life, bool fromPlayer, float slowSeconds = 0f, float slowFactor = 1f)
        {
            Pos = pos;
            Vel = vel;
            Radius = radius;
            Damage = damage;
            Life = life;
            FromPlayer = fromPlayer;
            SlowSeconds = slowSeconds;
            SlowFactor = slowFactor;
        }
    }

    // ---------- WORLD ----------
    public sealed class GameWorld
    {
        public readonly InputState Input = new();

        private readonly char[,] _tiles;
        private readonly int _w, _h;
        private const int TileSize = 48;
        private Vector2? _bossSpawnPos;

        private readonly Player _player;
        private readonly List<Enemy> _enemies = new();
        private readonly List<Projectile> _projectiles = new();
        private float _attackFaceTimer;
        private Boss? _boss;
        private bool _levelWon;

        private bool _inventoryOpen;

        private float _swingTimer = 0f;
        private const float SwingDuration = 0.16f;          // длительность удара
        private const float SwingArc = MathF.PI * 1.25f;    
        private float _swingStartAngle = 0f;

        private const float OFF_MELEE_UP_AND_FLIPPED = MathF.PI / 2f;
        private const float OFF_BOW_LEFT = MathF.PI;                  

        // визуалки/сообщения
        private float _attackFlash;
        private string _toast = "";
        private float _toastTimer;
        private readonly Brush _floorBrush = new SolidBrush(Color.FromArgb(255, 25, 25, 25));

        private readonly Random _rng = new();

        // ----- CAMERA -----
        private Vector2 _camTopLeft;          // верхний левый угол камеры в мировых координатах (px)
        private bool _camInited = false;
        private const float Zoom = 3.5f;      // зум камеры

        private static float Clamp(float v, float min, float max)
    => v < min ? min : (v > max ? max : v);
        private Vector2 WorldToScreen(Vector2 world)
        {
            return new Vector2(
                (world.X - _camTopLeft.X) * Zoom,
                (world.Y - _camTopLeft.Y) * Zoom
            );
        }
        private bool HasLineOfSight(Vector2 a, Vector2 b)
        {
            // DDA по линии (шаг 8 пикселей)
            Vector2 d = b - a;
            float len = d.Length();
            if (len < 0.001f) return true;

            Vector2 step = d / len * 8f;
            int n = (int)(len / 8f);

            Vector2 p = a;
            for (int i = 0; i <= n; i++)
            {
                int tx = (int)(p.X / TileSize);
                int ty = (int)(p.Y / TileSize);
                if (tx < 0 || ty < 0 || tx >= _w || ty >= _h) return false;
                if (_tiles[tx, ty] == '#') return false;
                p += step;
            }
            return true;
        }

        private static void DrawHpBarScreen(Graphics g, float cx, float cy, float t)
        {
            t = MathF.Max(0, MathF.Min(1, t));

            float barW = 46f;
            float barH = 6f;

            float x = cx - barW / 2f;
            float y = cy;

            g.FillRectangle(Brushes.Black, x, y, barW, barH);
            g.FillRectangle(Brushes.LimeGreen, x, y, barW * t, barH);
            g.DrawRectangle(Pens.White, x, y, barW, barH);
        }

        private void UpdateCamera(float dt, Size viewportPx)
        {
            // сколько “мира” видно в окне с учётом зума
            float viewW = viewportPx.Width / Zoom;
            float viewH = viewportPx.Height / Zoom;

            float mapW = _w * TileSize;
            float mapH = _h * TileSize;

            // центрировать игрока
            float targetX = _player.Pos.X - viewW / 2f;
            float targetY = _player.Pos.Y - viewH / 2f;

            // ограничение, чтобы камера не улетала за карту
            targetX = Clamp(targetX, 0, MathF.Max(0, mapW - viewW));
            targetY = Clamp(targetY, 0, MathF.Max(0, mapH - viewH));

            if (!_camInited)
            {
                _camTopLeft = new Vector2(targetX, targetY);
                _camInited = true;
                return;
            }

            // плавное следование
            float k = 1f - MathF.Exp(-12f * dt);
            _camTopLeft = Vector2.Lerp(_camTopLeft, new Vector2(targetX, targetY), k);
        }


        private GameWorld(char[,] tiles, Player player, List<Enemy> enemies)
        {
            _tiles = tiles;
            _w = tiles.GetLength(0);
            _h = tiles.GetLength(1);
            _player = player;
            _enemies = enemies;
        }
        private void SpawnBossIfReady()
        {
            if (_boss != null) return;
            if (!_bossSpawnPos.HasValue) return;

            // если враги все мертвы — спавним
            if (_enemies.Count == 0)
            {
                var bt = (BossType)_rng.Next(3);
                _boss = new Boss(bt, _bossSpawnPos.Value);
                Toast($"Появился босс: {bt}");
            }
        }

        public void OnKeyPressed(Keys key)
        {
            if (key == Keys.I)
            {
                _inventoryOpen = !_inventoryOpen;
                Toast(_inventoryOpen ? "Инвентарь открыт (I закрыть). 1-9 экипировать предмет." : "Инвентарь закрыт");
                return;
            }

            if (_inventoryOpen)
            {
                // 1-9: экипировать предмет из списка
                int idx = key switch
                {
                    Keys.D1 => 1,
                    Keys.D2 => 2,
                    Keys.D3 => 3,
                    Keys.D4 => 4,
                    Keys.D5 => 5,
                    Keys.D6 => 6,
                    Keys.D7 => 7,
                    Keys.D8 => 8,
                    Keys.D9 => 9,
                    _ => 0
                };
                if (idx > 0)
                {
                    int i = idx - 1;
                    if (i >= 0 && i < _player.Inv.Items.Count)
                    {
                        var it = _player.Inv.Items[i];
                        _player.Inv.Equip(it, _player);
                        _player.RecomputeStats();
                        Toast($"Экипировано: {it.Name}");
                    }
                }

            }
            else
            {
                // Быстрый свитч базового оружия 1-4 (сбрасывает “оружейный предмет”)
                if (key == Keys.D1) { _player.Inv.EquippedWeaponItem = null; _player.EquippedWeapon = WeaponType.Sword; Toast("Оружие: Меч"); }
                if (key == Keys.D2) { _player.Inv.EquippedWeaponItem = null; _player.EquippedWeapon = WeaponType.Axe; Toast("Оружие: Топор"); }
                if (key == Keys.D3) { _player.Inv.EquippedWeaponItem = null; _player.EquippedWeapon = WeaponType.Spear; Toast("Оружие: Копьё"); }
                if (key == Keys.D4) { _player.Inv.EquippedWeaponItem = null; _player.EquippedWeapon = WeaponType.Bow; Toast("Оружие: Лук"); }
            }
        }

        public static GameWorld FromMap(string[] lines)
        {
            int h = lines.Length;

            // берём ширину как МАКСИМАЛЬНУЮ длину строки (а не lines[0])
            int w = 0;
            for (int i = 0; i < h; i++)
            {
                if (lines[i] == null) lines[i] = "";
                if (lines[i].Length > w) w = lines[i].Length;
            }
            Vector2? bossPos = null;

            var tiles = new char[w, h];
            Vector2 playerPos = new(1 * TileSize + TileSize / 2, 1 * TileSize + TileSize / 2);
            var enemies = new List<Enemy>();
            Vector2? bossSpawnPos = null;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    char c = (x < lines[y].Length) ? lines[y][x] : '#';

                    if (c == 'P')
                    {
                        playerPos = new(x * TileSize + TileSize / 2, y * TileSize + TileSize / 2);
                        c = '.';
                    }
                    else if (c == 'E') // зомби
                    {
                        var epos = new Vector2(x * TileSize + TileSize / 2, y * TileSize + TileSize / 2);
                        enemies.Add(new Enemy(EnemyType.Zombie, epos));
                        c = '.';
                    }
                    else if (c == 'R') // скелет-лучник
                    {
                        var epos = new Vector2(x * TileSize + TileSize / 2, y * TileSize + TileSize / 2);
                        enemies.Add(new Enemy(EnemyType.SkeletonArcher, epos));
                        c = '.';
                    }
                    else if (c == 'S') // паук
                    {
                        var epos = new Vector2(x * TileSize + TileSize / 2, y * TileSize + TileSize / 2);
                        enemies.Add(new Enemy(EnemyType.Spider, epos));
                        c = '.';
                    }
                    else if (c == 'B')
                    {
                        bossSpawnPos = new Vector2(x * TileSize + TileSize / 2, y * TileSize + TileSize / 2);
                        c = '.';
                    }

                    tiles[x, y] = c;
                }
            }

            var world = new GameWorld(tiles, new Player(playerPos), enemies);
            world._bossSpawnPos = bossSpawnPos;
            GameArt.LoadOnce();

            if (bossPos.HasValue)
            {
                // случайный босс из 3
                var bt = (BossType)world._rng.Next(3);
                world._boss = new Boss(bt, bossPos.Value);
                world.Toast($"Босс на карте: {bt}");
            }

            return world;
        }

        public void Update(float dt)
        {
            if (_player.IsDead)
            {
                _player.Anim.Play(AnimState.Death);
                _player.Anim.Frame = 0;
                Input.EndFrame();
                return;
            }

            // сообщение
            _swingTimer = MathF.Max(0f, _swingTimer - dt);
            _toastTimer = MathF.Max(0, _toastTimer - dt);
            if (_toastTimer <= 0) _toast = "";

            // если открыт инвентарь — “пауза”
            if (_inventoryOpen)
            {
                Input.EndFrame();
                return;
            }

            if (_levelWon)
            {
                Input.EndFrame();
                return;
            }

            // пересчёт статов от экипировки
            _player.RecomputeStats();

            // timers
            var weapon = _player.GetWeaponData();
            _player.AttackCd = MathF.Max(0, _player.AttackCd - dt);
            _player.InvulnTimer = MathF.Max(0, _player.InvulnTimer - dt);
            _attackFlash = MathF.Max(0, _attackFlash - dt);

            _player.SlowTimer = MathF.Max(0, _player.SlowTimer - dt);
            if (_player.SlowTimer <= 0) _player.SlowFactor = 1f;
            _player.RollTimer = MathF.Max(0, _player.RollTimer - dt);

            for (int i = 0; i < _enemies.Count; i++)
                _enemies[i].AttackCd = MathF.Max(0, _enemies[i].AttackCd - dt);

            // move player
            Vector2 dir = Vector2.Zero;
            if (Input.W) dir.Y -= 1;
            if (Input.S) dir.Y += 1;
            if (Input.A) dir.X -= 1;
            if (Input.D) dir.X += 1;
            if (dir.LengthSquared() > 0) dir = Vector2.Normalize(dir);

            float speed = _player.Stats.Speed;
            speed *= _player.SlowFactor;

            Vector2 moveDir = dir;

            if (_player.RollTimer > 0)
            {
                moveDir = _player.RollMoveDir;
                speed *= 2.0f;
            }

            var nextPlayer = _player.Pos + moveDir * speed * dt;
            if (!IsWallAt(nextPlayer))
                _player.Pos = nextPlayer;

            // roll start
            if (Input.Space && _player.InvulnTimer <= 0)
            {
                _player.InvulnTimer = 0.35f;
                _player.RollTimer = 0.35f;

                // направление переката: если есть ввод WASD — туда, иначе в сторону взгляда
                Vector2 rollDir = dir.LengthSquared() > 0.0001f
                    ? Vector2.Normalize(dir)
                    : new Vector2(MathF.Cos(_player.AimAngleRad), MathF.Sin(_player.AimAngleRad));

                _player.RollMoveDir = rollDir;

                // фиксируем "картинное" направление переката (row в спрайте)
                _player.RollDir = GameArt.DirFromAngle(MathF.Atan2(rollDir.Y, rollDir.X));
            }


            var nextPlayer2 = _player.Pos + dir * speed * dt;
            if (!IsWallAt(nextPlayer2))
                _player.Pos = nextPlayer2;

            // aim
            Vector2 mouseWorld = _camTopLeft + Input.MousePos / Zoom;
            Vector2 toMouse = mouseWorld - _player.Pos;
            _player.AimAngleRad = MathF.Atan2(toMouse.Y, toMouse.X);

            if (toMouse.LengthSquared() > 0.001f)
                _player.AimAngleRad = MathF.Atan2(toMouse.Y, toMouse.X);

            _player.Dir = GameArt.DirFromAngle(_player.AimAngleRad);

            // если идёт анимация атаки — держим её чуть-чуть
            _player.AttackAnimLock = MathF.Max(0, _player.AttackAnimLock - dt);

            _attackFlash = 0.12f;
            _attackFaceTimer = MathF.Max(0, _attackFaceTimer - dt);

            bool moving = dir.LengthSquared() > 0.0001f;

            if (_player.RollTimer > 0)
            {
                _player.Anim.Play(AnimState.Roll);
                _player.Anim.Update(dt, frameCount: 4, fps: 14f, loop: false);
            }
            else
            {
                _player.Anim.Play(moving ? AnimState.Walk : AnimState.Idle);
                _player.Anim.Update(dt, frameCount: moving ? 6 : 1, fps: moving ? 10f : 1f, loop: true);
            }

            // обычное направление (для walk)
            _player.Dir = GameArt.DirFromAngle(_player.AimAngleRad);


            // attack
            if (Input.MouseLeftPressedThisFrame && _player.AttackCd <= 0)
            {
                DoPlayerAttack(weapon);
                _player.AttackCd = weapon.CooldownSeconds;

                if (!weapon.IsRanged)
                {
                    _swingTimer = SwingDuration;
                    _swingStartAngle = _player.AimAngleRad - SwingArc * 0.5f;
                }
            }


            // open chest if standing on it
            TryOpenChestUnderPlayer();

            // projectiles
            for (int p = _projectiles.Count - 1; p >= 0; p--)
            {
                var pr = _projectiles[p];
                pr.Life -= dt;
                if (pr.Life <= 0) { _projectiles.RemoveAt(p); continue; }

                var next = pr.Pos + pr.Vel * dt;
                if (IsWallAt(next)) { _projectiles.RemoveAt(p); continue; }

                pr.Pos = next;

                if (pr.FromPlayer)
                {
                    bool hit = false;
                    for (int i = 0; i < _enemies.Count; i++)
                    {
                        var e = _enemies[i];
                        if (e.IsDead) continue;

                        float d = Vector2.Distance(e.Pos, pr.Pos);
                        if (d <= e.Radius + pr.Radius)
                        {
                            e.TakeDamage(pr.Damage);
                            hit = true;
                            break;
                        }
                    }
                    if (hit) { _projectiles.RemoveAt(p); continue; }
                }
                else
                {
                    float d = Vector2.Distance(_player.Pos, pr.Pos);
                    if (d <= _player.Radius + pr.Radius)
                    {
                        if (_player.InvulnTimer <= 0)
                            _player.TakeDamage(pr.Damage);

                        if (pr.SlowSeconds > 0)
                        {
                            _player.SlowTimer = pr.SlowSeconds;
                            _player.SlowFactor = pr.SlowFactor;
                        }

                        _projectiles.RemoveAt(p);
                        continue;
                    }
                }

                if (_boss != null && !_boss.IsDead)
                {
                    float dB = Vector2.Distance(_boss.Pos, pr.Pos);
                    if (dB <= _boss.Radius + pr.Radius)
                    {
                        _boss.TakeDamage(pr.Damage);
                        _projectiles.RemoveAt(p);
                        continue;
                    }
                }
            }

            // enemies AI
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var e = _enemies[i];
                if (e.IsDead) { _enemies.RemoveAt(i); continue; }
                if (_boss == null && _bossSpawnPos.HasValue && _enemies.Count == 0)
                {
                    var bt = (BossType)_rng.Next(3);
                    _boss = new Boss(bt, _bossSpawnPos.Value);
                    Toast($"Появился босс: {bt}");
                }

                Vector2 toP = _player.Pos - e.Pos;
                float dist = toP.Length();
                Vector2 edir = dist > 0.001f ? (toP / dist) : Vector2.Zero;
                e.AimAngleRad = MathF.Atan2(edir.Y, edir.X);

                switch (e.Type)
                {
                    case EnemyType.Zombie:
                        {
                            Vector2 prevPos = e.Pos;
                            if (dist > e.AttackRange * 0.9f)
                            {
                                var nextE = e.Pos + edir * e.Stats.Speed * dt;
                                if (!IsWallAt(nextE)) e.Pos = nextE;
                            }

                            if (dist <= e.AttackRange + _player.Radius && e.AttackCd <= 0)
                            {
                                if (!HasLineOfSight(e.Pos, _player.Pos)) continue;
                                if (_player.InvulnTimer <= 0) _player.TakeDamage(e.Damage);
                                e.AttackCd = e.AttackCooldownSeconds;
                            }
                            e.Dir = GameArt.DirFromAngle(MathF.Atan2(edir.Y, edir.X));

                            bool moved = Vector2.DistanceSquared(e.Pos, prevPos) > 0.1f;

                            // направление — куда “смотрит/идёт”
                            if (dist > 0.001f)
                                e.Dir = GameArt.DirFromAngle(MathF.Atan2(edir.Y, edir.X));

                            // анимация
                            if (moved)
                            {
                                e.Anim.Play(AnimState.Walk);
                                e.Anim.Update(dt, frameCount: 6, fps: 9f, loop: true);
                            }
                            else
                            {
                                e.Anim.Play(AnimState.Idle);
                                e.Anim.Frame = 0;
                            }
                            break;
                        }

                    case EnemyType.SkeletonArcher:
                        {
                            if (dist < e.PreferredRange * 0.85f)
                            {
                                var nextE = e.Pos - edir * e.Stats.Speed * dt;
                                if (!IsWallAt(nextE)) e.Pos = nextE;
                            }
                            else if (dist > e.PreferredRange * 1.15f)
                            {
                                var nextE = e.Pos + edir * e.Stats.Speed * dt;
                                if (!IsWallAt(nextE)) e.Pos = nextE;
                            }

                            if (dist <= e.AttackRange && e.AttackCd <= 0)
                            {
                                Vector2 forward = edir;
                                Vector2 spawn = e.Pos + forward * (e.Radius + 10f);
                                Vector2 vel = forward * e.ProjectileSpeed;
                                if (!HasLineOfSight(e.Pos, _player.Pos)) break;
                                _projectiles.Add(new Projectile(
                                    pos: spawn, vel: vel, radius: 5f,
                                    damage: e.Damage, life: 2.2f,
                                    fromPlayer: false
                                ));

                                e.AttackCd = e.AttackCooldownSeconds;
                            }
                            e.Dir = GameArt.DirFromAngle(MathF.Atan2(edir.Y, edir.X));

                            e.Anim.Play(dist > 2f ? AnimState.Walk : AnimState.Idle);
                            e.Anim.Update(dt, frameCount: 6, fps: 9f, loop: true);
                            break;
                        }

                    case EnemyType.Spider:
                        {
                            if (dist < e.PreferredRange * 0.75f)
                            {
                                var nextE = e.Pos - edir * e.Stats.Speed * dt;
                                if (!IsWallAt(nextE)) e.Pos = nextE;
                            }
                            else if (dist > e.PreferredRange * 1.25f)
                            {
                                var nextE = e.Pos + edir * e.Stats.Speed * dt;
                                if (!IsWallAt(nextE)) e.Pos = nextE;
                            }

                            // паутина (замедляет)
                            if (dist <= 260f && e.AttackCd <= 0)
                            {
                                Vector2 forward = edir;
                                Vector2 spawn = e.Pos + forward * (e.Radius + 10f);
                                Vector2 vel = forward * e.ProjectileSpeed;
                                if (!HasLineOfSight(e.Pos, _player.Pos)) break;
                                _projectiles.Add(new Projectile(
                                    pos: spawn, vel: vel, radius: 6f,
                                    damage: 3f, life: 2.0f,
                                    fromPlayer: false,
                                    slowSeconds: 2.2f, slowFactor: 0.55f
                                ));

                                e.AttackCd = e.AttackCooldownSeconds;
                            }

                            // укус вблизи (используем тот же CD для простоты)
                            if (dist <= e.AttackRange + _player.Radius && e.AttackCd <= 0)
                            {
                                if (!HasLineOfSight(e.Pos, _player.Pos)) continue;
                                if (_player.InvulnTimer <= 0) _player.TakeDamage(e.Damage);
                                e.AttackCd = e.AttackCooldownSeconds;
                            }
                            e.Dir = GameArt.DirFromAngle(MathF.Atan2(edir.Y, edir.X));

                            e.Anim.Play(dist > 2f ? AnimState.Walk : AnimState.Idle);
                            e.Anim.Update(dt, frameCount: 6, fps: 9f, loop: true);
                            break;
                        }

                }
            }

            UpdateBoss(dt);

            UpdateCamera(dt, new Size(1920, 1080));

            _enemies.RemoveAll(e => e.IsDead);
            SpawnBossIfReady();

            Input.EndFrame();
        }

        private void UpdateBoss(float dt)
        {
            if (_boss == null) return;
            if (_boss.IsDead)
            {
                if (!_levelWon)
                {
                    _levelWon = true;
                    Toast("ПОБЕДА! ESC — в меню");
                }
                return;
            }

            var b = _boss;

            b.MeleeCd = MathF.Max(0, b.MeleeCd - dt);
            b.SpecialCd = MathF.Max(0, b.SpecialCd - dt);
            b.Windup = MathF.Max(0, b.Windup - dt);

            Vector2 toP = _player.Pos - b.Pos;
            float dist = toP.Length();
            Vector2 dir = dist > 0.001f ? (toP / dist) : Vector2.Zero;

            // движение (босс медленный, но давит)
            if (dist > b.MeleeRange * 0.9f)
            {
                var next = b.Pos + dir * b.MoveSpeed * dt;
                if (!IsWallAt(next))
                    b.Pos = next;
            }

            // обычная атака
            if (dist <= b.MeleeRange + _player.Radius && b.MeleeCd <= 0)
            {
                if (_player.InvulnTimer <= 0)
                    _player.TakeDamage(b.MeleeDamage);

                b.MeleeCd = 1.0f; // 1 удар/сек
            }

            // особые атаки
            if (b.SpecialCd <= 0)
            {
                switch (b.Type)
                {
                    case BossType.GraveBrute:
                        {
                            // Удар по земле: телеграф 0.6с, потом урон в радиусе
                            b.Windup = 0.6f;
                            b.StoredPos = b.Pos;
                            b.SpecialCd = 4.0f;
                            break;
                        }
                    case BossType.BoneLord:
                        {
                            // Веер стрел
                            int n = 7;
                            float spread = 0.55f; // радианы
                            float baseAng = MathF.Atan2(dir.Y, dir.X);
                            for (int i = 0; i < n; i++)
                            {
                                float t = (n == 1) ? 0 : (i / (float)(n - 1));
                                float ang = baseAng + (t - 0.5f) * spread;
                                Vector2 f = new Vector2(MathF.Cos(ang), MathF.Sin(ang));

                                _projectiles.Add(new Projectile(
                                    pos: b.Pos + f * (b.Radius + 10f),
                                    vel: f * 520f,
                                    radius: 6f,
                                    damage: 10f,
                                    life: 2.2f,
                                    fromPlayer: false
                                ));
                            }
                            b.SpecialCd = 3.6f;
                            break;
                        }
                    case BossType.WebQueen:
                        {
                            // Тройной залп паутины (замедляет)
                            int n = 3;
                            float spread = 0.35f;
                            float baseAng = MathF.Atan2(dir.Y, dir.X);
                            for (int i = 0; i < n; i++)
                            {
                                float t = (n == 1) ? 0 : (i / (float)(n - 1));
                                float ang = baseAng + (t - 0.5f) * spread;
                                Vector2 f = new Vector2(MathF.Cos(ang), MathF.Sin(ang));

                                _projectiles.Add(new Projectile(
                                    pos: b.Pos + f * (b.Radius + 10f),
                                    vel: f * 420f,
                                    radius: 8f,
                                    damage: 5f,
                                    life: 2.0f,
                                    fromPlayer: false,
                                    slowSeconds: 3.0f,
                                    slowFactor: 0.55f
                                ));
                            }
                            b.SpecialCd = 4.2f;
                            break;
                        }
                }
            }

            // выполнение “удара по земле” после телеграфа
            if (b.Type == BossType.GraveBrute && b.Windup > 0 && b.Windup <= 0.001f)
            {
                
            }
            if (b.Type == BossType.GraveBrute && b.Windup > 0)
            {
                
            }
            else if (b.Type == BossType.GraveBrute && b.StoredPos != default && b.SpecialCd > 0) 
            {
                float shockR = 95f;
                float d = Vector2.Distance(_player.Pos, b.StoredPos);
                if (d <= shockR + _player.Radius)
                {
                    if (_player.InvulnTimer <= 0)
                        _player.TakeDamage(20f);
                }
                b.StoredPos = default;
            }
        }

        private void DoPlayerAttack(WeaponData w)
        {
            _attackFlash = 0.12f;

            Vector2 forward = new(MathF.Cos(_player.AimAngleRad), MathF.Sin(_player.AimAngleRad));

            if (w.IsRanged)
            {
                Vector2 spawn = _player.Pos + forward * w.Range;
                Vector2 vel = forward * w.ProjectileSpeed;

                _projectiles.Add(new Projectile(
                    pos: spawn, vel: vel, radius: 6f,
                    damage: w.Damage, life: 1.8f,
                    fromPlayer: true
                ));
                return;
            }

            for (int i = 0; i < _enemies.Count; i++)
            {
                var e = _enemies[i];
                if (e.IsDead) continue;

                Vector2 toE = e.Pos - _player.Pos;
                float dist = toE.Length();
                if (dist > w.Range + e.Radius) continue;

                Vector2 dir = dist > 0.001f ? (toE / dist) : Vector2.Zero;
                float dot = Vector2.Dot(forward, dir);
                if (dot < w.ConeDot) continue;
                if (!HasLineOfSight(_player.Pos, e.Pos)) continue;
                e.TakeDamage(w.Damage);
            }

            if (_boss != null && !_boss.IsDead)
            {
                forward = new(MathF.Cos(_player.AimAngleRad), MathF.Sin(_player.AimAngleRad));
                Vector2 toB = _boss.Pos - _player.Pos;
                float dist = toB.Length();
                if (dist <= w.Range + _boss.Radius)
                {
                    Vector2 ddir = dist > 0.001f ? (toB / dist) : Vector2.Zero;
                    float dot = Vector2.Dot(forward, ddir);
                    if (dot >= w.ConeDot)
                        _boss.TakeDamage(w.Damage);
                }
            }
        }

        private void TryOpenChestUnderPlayer()
        {
            int tx = (int)(_player.Pos.X / TileSize);
            int ty = (int)(_player.Pos.Y / TileSize);
            if (tx < 0 || ty < 0 || tx >= _w || ty >= _h) return;

            if (_tiles[tx, ty] != 'C') return;

            _tiles[tx, ty] = '.'; // сундук открыт

            int drops = RollChestDrops(); // 1-3
            int equippedCount = 0;

            for (int k = 0; k < drops; k++)
            {
                var loot = GenerateRandomItem();
                _player.Inv.Add(loot);

                if (TryAutoEquip(loot))
                    equippedCount++;
            }

            _player.RecomputeStats();
            _player.Stats.Hp += 25;

            Toast(equippedCount > 0
                ? $"Сундук: выпало {drops} | автоэкип {equippedCount}"
                : $"Сундук: выпало {drops} (I — инвентарь)");
        }

        private bool TryAutoEquip(Item it)
        {
            // попытка автоэкипировать только если лучше
            float score = it.Score();

            if (it.Slot == ItemSlot.Weapon)
            {
                float cur = _player.Inv.EquippedWeaponItem?.Score() ?? -999f;
                if (score > cur)
                {
                    _player.Inv.Equip(it, _player);
                    return true;
                }
            }
            else if (it.Slot == ItemSlot.Armor)
            {
                float cur = _player.Inv.EquippedArmor?.Score() ?? -999f;
                if (score > cur)
                {
                    _player.Inv.Equip(it, _player);
                    return true;
                }
            }
            else
            {
                float cur = _player.Inv.EquippedAccessory?.Score() ?? -999f;
                if (score > cur)
                {
                    _player.Inv.Equip(it, _player);
                    return true;
                }
            }

            return false;
        }

        private int RollChestDrops()
        {
            int r = _rng.Next(100);
            if (r < 15) return 3;
            if (r < 55) return 2;
            return 1;
        }

        private ItemRarity RollRarity()
        {
            int r = _rng.Next(1000);
            if (r < 10) return ItemRarity.Legendary;    // 1%
            if (r < 50) return ItemRarity.Epic;         // 5%
            if (r < 250) return ItemRarity.Rare;        // 25%
            if (r < 500) return ItemRarity.Uncommon;    // 50%
            return ItemRarity.Common;                   
        }

        private Item GenerateRandomItem()
        {
            var rarity = RollRarity();
            float m = Item.RarityMult(rarity);

            int roll = _rng.Next(100);

            Item it;

            if (roll < 35)
            {
                it = _rng.Next(3) switch
                {
                    0 => new Item { Name = "Кольцо скорости", Slot = ItemSlot.Accessory, AddSpeed = 25 * m },
                    1 => new Item { Name = "Амулет здоровья", Slot = ItemSlot.Accessory, AddMaxHp = 25 * m },
                    2 => new Item { Name = "Браслет ловкости", Slot = ItemSlot.Accessory, AddSpeed = 15 * m, AddArmor = 2 * m },
                    _ => new Item { Name = "Кольцо атаки", Slot = ItemSlot.Accessory, AddDamage = 4 * m },
                };
            }
            else if (roll < 65)
            {
                it = _rng.Next(2) switch
                {
                    0 => new Item { Name = "Кожаная броня", Slot = ItemSlot.Armor, AddArmor = 2 * m, AddSpeed = -10 },
                    1 => new Item { Name = "Железная броня", Slot = ItemSlot.Armor, AddArmor = 8 * m, AddSpeed = -30, AddMaxHp = 25 * m },
                    _ => new Item { Name = "Кольчуга", Slot = ItemSlot.Armor, AddArmor = 4 * m, AddSpeed = -20, AddMaxHp = 10 * m },
                };
            }
            else
            {
                it = _rng.Next(4) switch
                {
                    0 => new Item { Name = "Короткий меч", Slot = ItemSlot.Weapon, WeaponType = WeaponType.Sword, AddDamage = 6 * m, AddAttackSpeed = 0.2f * (m - 0.9f) },
                    1 => new Item { Name = "Тяжёлый топор", Slot = ItemSlot.Weapon, WeaponType = WeaponType.Axe, AddDamage = 10 * m, AddAttackSpeed = -0.1f },
                    2 => new Item { Name = "Длинное копьё", Slot = ItemSlot.Weapon, WeaponType = WeaponType.Spear, AddRange = 25 * m, AddDamage = 4 * m },
                    _ => new Item { Name = "Охотничий лук", Slot = ItemSlot.Weapon, WeaponType = WeaponType.Bow, AddDamage = 5 * m, AddAttackSpeed = 0.1f * (m - 0.8f) },
                };
            }

            it.Rarity = rarity;
            it.Name = $"{it.Name} [{it.RarityTag}]";
            return it;
        }

        private void Toast(string text)
        {
            _toast = text;
            _toastTimer = 2.2f;
        }

        private bool IsWallAt(Vector2 p)
        {
            int tx = (int)(p.X / TileSize);
            int ty = (int)(p.Y / TileSize);
            if (tx < 0 || ty < 0 || tx >= _w || ty >= _h) return true;
            return _tiles[tx, ty] == '#';
        }

        public void Draw(Graphics g, Size client)
        {
            g.Clear(Color.Black);
            var hudFont = FontManager.Px(14);
            // пиксельная отрисовка
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

            //если камера ещё не обновилась
            if (!_camInited)
                _camTopLeft = Vector2.Zero;

            // сколько мира видно при зуме
            float viewW = client.Width / Zoom;
            float viewH = client.Height / Zoom;

            // видимый диапазон тайлов (рисуем только то, что в камере)
            int x0 = Math.Max(0, (int)MathF.Floor(_camTopLeft.X / TileSize) - 1);
            int y0 = Math.Max(0, (int)MathF.Floor(_camTopLeft.Y / TileSize) - 1);
            int x1 = Math.Min(_w - 1, (int)MathF.Ceiling((_camTopLeft.X + viewW) / TileSize) + 1);
            int y1 = Math.Min(_h - 1, (int)MathF.Ceiling((_camTopLeft.Y + viewH) / TileSize) + 1);

            // сохраняем состояние и применяем transform: сначала сдвиг камеры, потом зум
            var state = g.Save();
            var m = new System.Drawing.Drawing2D.Matrix();
            m.Translate(-_camTopLeft.X, -_camTopLeft.Y);
            m.Scale(Zoom, Zoom, System.Drawing.Drawing2D.MatrixOrder.Append);
            g.Transform = m;

            // tiles
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    char c = _tiles[x, y];
                    var r = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);

                    if (c == '#')
                    {
                        if (GameArt.Wall.Loaded)
                            g.DrawImage(GameArt.Wall.Image!, r, GameArt.Wall.Src(0, 0), GraphicsUnit.Pixel);
                        else
                            g.FillRectangle(Brushes.DimGray, r);
                    }
                    else
                    {
                        if (GameArt.Floor.Loaded)
                            g.DrawImage(GameArt.Floor.Image!, r, GameArt.Floor.Src(0, 0), GraphicsUnit.Pixel);
                        else
                            g.FillRectangle(_floorBrush, r);
                    }

                    // сундук/прочее поверх
                    if (c == 'C')
                    {
                        if (GameArt.Chest.Loaded)
                            g.DrawImage(GameArt.Chest.Image!, r, GameArt.Chest.Src(0, 0), GraphicsUnit.Pixel);
                        else
                            g.FillEllipse(Brushes.Goldenrod, r);
                    }

                }
            }

            // ================== ENEMIES DRAW (WORLD SPACE) ==================
            foreach (var e in _enemies)
            {
                if (e.IsDead) continue;

                //Выбираем спрайтшит под тип врага
                SpriteSheet esheet = e.Type switch
                {
                    EnemyType.Zombie => GameArt.ZombieWalk,
                    EnemyType.SkeletonArcher => GameArt.SkeletonWalk,
                    EnemyType.Spider => GameArt.SpiderWalk,
                    _ => GameArt.ZombieWalk
                };


                float enemyScale = 2.0f;
                float ew = GameArt.FrameW * enemyScale;
                float eh = GameArt.FrameH * enemyScale;

                RectangleF edst = new RectangleF(e.Pos.X - ew / 2f, e.Pos.Y - eh / 2f, ew, eh);

                if (esheet.Loaded)
                {
                    // направление врага e.Dir, кадр из e.Anim.Frame
                    int col = e.Anim.Frame % esheet.Cols;
                    int row = (int)e.Dir;
                    GameArt.DrawSheetFrame(g, esheet, col, row, edst);
                }
                else
                {
                    
                    Brush b =
                        e.Type == EnemyType.SkeletonArcher ? Brushes.Gainsboro :
                        e.Type == EnemyType.Spider ? Brushes.LawnGreen :
                        Brushes.IndianRed;

                    g.FillEllipse(b, e.Pos.X - e.Radius, e.Pos.Y - e.Radius, e.Radius * 2, e.Radius * 2);
                }

                // Лук у скелета
                if (e.Type == EnemyType.SkeletonArcher && GameArt.WeaponBow.Loaded)
                {
                    float ang = e.AimAngleRad;

                    Vector2 fwd = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                    Vector2 bowPos = e.Pos + fwd * 18f;

                    float bowScale = 1.2f;
                    float bw = GameArt.FrameW * bowScale;
                    float bh = GameArt.FrameH * bowScale;

                    DrawRotatedSingle(g, GameArt.WeaponBow, bowPos, bw, bh, ang, OFF_BOW_LEFT);
                }
            }


            // projectiles
            foreach (var pr in _projectiles)
            {
                // web
                if (pr.SlowSeconds > 0)
                {
                    if (GameArt.Web.Loaded)
                    {
                        float a = MathF.Atan2(pr.Vel.Y, pr.Vel.X);
                        float webScale = 1.2f;
                        float ww = GameArt.FrameW * webScale;
                        float wh = GameArt.FrameH * webScale;

                        DrawRotatedSingle(g, GameArt.Web, pr.Pos, ww, wh, a);
                    }
                    else
                    {
                        g.FillEllipse(Brushes.LightGreen, pr.Pos.X - pr.Radius, pr.Pos.Y - pr.Radius, pr.Radius * 2, pr.Radius * 2);
                    }
                    continue;
                }


                // arrow
                if (GameArt.Arrow.Loaded)
                {
                    float a = MathF.Atan2(pr.Vel.Y, pr.Vel.X);

                    float arrowScale = 1.2f;
                    float aw = GameArt.FrameW * arrowScale;
                    float ah = GameArt.FrameH * arrowScale;

                    DrawRotatedSingle(g, GameArt.Arrow, pr.Pos, aw, ah, a);
                }
                else
                {
                    g.FillEllipse(Brushes.White, pr.Pos.X - pr.Radius, pr.Pos.Y - pr.Radius, pr.Radius * 2, pr.Radius * 2);
                }
            }


            // player
            float scale = 2.0f;
            float dw = GameArt.FrameW * scale;
            float dh = GameArt.FrameH * scale;

            RectangleF dst = new RectangleF(_player.Pos.X - dw / 2, _player.Pos.Y - dh / 2, dw, dh);
            bool rotatePlayer = (_attackFaceTimer > 0) && (_player.Anim.State == AnimState.Idle);
            SpriteSheet pSheet;
            int pRow;
            int pCol;
            
            if (rotatePlayer && GameArt.PlayerIdle.Loaded)
            {
                DrawRotatedSingle(g, GameArt.PlayerIdle, _player.Pos, dw, dh, _player.AimAngleRad);
            }
            if (_player.Anim.State == AnimState.Death)
            {
                pSheet = GameArt.PlayerDeath;
                pRow = 0;
                pCol = 0;
            }
            else if (_player.Anim.State == AnimState.Roll)
            {
                pSheet = GameArt.PlayerRoll;
                pRow = (int)_player.RollDir;
                pCol = _player.Anim.Frame;
            }
            else if (_player.Anim.State == AnimState.Walk)
            {
                pSheet = GameArt.PlayerWalk;
                pRow = (int)_player.Dir;
                pCol = _player.Anim.Frame;
            }
            else
            {
                pSheet = GameArt.PlayerIdle;
                pRow = 0;
                pCol = 0;
            }

            if (pSheet.Loaded)
                GameArt.DrawSheetFrame(g, pSheet, pCol, pRow, dst);
            else
                g.FillEllipse(Brushes.DeepSkyBlue, _player.Pos.X - _player.Radius, _player.Pos.Y - _player.Radius, _player.Radius * 2, _player.Radius * 2);

            // ---- WEAPON DRAW ----
            if (!_player.IsDead)
            {
                var wd = _player.GetWeaponData();
                float aim = _player.AimAngleRad;

                // размеры спрайта оружия (в мировых единицах)
                float weaponScale = 1.5f;
                float ww = GameArt.FrameW * weaponScale;
                float wh = GameArt.FrameH * weaponScale;

                // позиция “руки” в обычном режиме
                Vector2 aimFwd = new(MathF.Cos(aim), MathF.Sin(aim));
                float handDist = 22f;

                if (_player.EquippedWeapon == WeaponType.Bow)
                {
                    Vector2 bowPos = _player.Pos + aimFwd * handDist;

                    if (GameArt.WeaponBow.Loaded)
                        DrawRotatedSingle(g, GameArt.WeaponBow, bowPos, ww, wh, aim, OFF_BOW_LEFT);
                }
                else
                {
                    float ang;
                    float radius;

                    if (_swingTimer > 0f)
                    {
                        float p = 1f - (_swingTimer / SwingDuration);     // 0..1
                        ang = _swingStartAngle + p * SwingArc;            // облет по дуге
                        radius = wd.Range;                                // радиус удара
                    }
                    else
                    {
                        ang = aim;
                        radius = handDist;                                // держим возле игрока, пока не бьём
                    }

                    Vector2 pos = _player.Pos + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * radius;

                    SpriteSheet ws = _player.EquippedWeapon switch
                    {
                        WeaponType.Sword => GameArt.WeaponSword,
                        WeaponType.Axe => GameArt.WeaponAxe,
                        WeaponType.Spear => GameArt.WeaponSpear,
                        _ => GameArt.WeaponSword
                    };

                    if (ws.Loaded)
                        DrawRotatedSingle(g, ws, pos, ww, wh, ang, OFF_MELEE_UP_AND_FLIPPED);
                }
            }

            // Boss draw
            if (_boss != null && !_boss.IsDead)
            {
                // направление босса
                Vector2 toP = _player.Pos - _boss.Pos;
                float dist = toP.Length();
                Vector2 dir = dist > 0.001f ? (toP / dist) : Vector2.UnitY;
                Dir4 bDir = GameArt.DirFromAngle(MathF.Atan2(dir.Y, dir.X));

                // какой спрайт использовать под каждого босса
                SpriteSheet bSheet = _boss.Type switch
                {
                    BossType.GraveBrute => GameArt.ZombieWalk,      
                    BossType.BoneLord => GameArt.SkeletonWalk,    
                    BossType.WebQueen => GameArt.SpiderWalk,      
                    _ => GameArt.ZombieWalk
                };

                // В 2 раза больше врага
                float enemyScale = 2.0f;
                float bossScale = enemyScale * 3.0f;

                float bw = GameArt.FrameW * bossScale;
                float bh = GameArt.FrameH * bossScale;

                // "земля" = нижняя точка хитбокса
                float groundY = _boss.Pos.Y + _boss.Radius;

                float feetLift = bh * 0.08f;

                var bossDst = new RectangleF(
                    _boss.Pos.X - bw / 2f,
                    (groundY - bh) + feetLift,
                    bw, bh
                );

                // анимация ходьбы
                int bCol = (int)(Environment.TickCount / 90) % Math.Max(1, bSheet.Cols); // ~11 fps
                int bRow = (int)bDir;

                if (bSheet.Loaded)
                    GameArt.DrawSheetFrame(g, bSheet, bCol, bRow, bossDst);
                else
                    g.FillEllipse(Brushes.SandyBrown, bossDst);
            }

            // look
            var lookLen = 28f;
            var look = new Vector2(MathF.Cos(_player.AimAngleRad), MathF.Sin(_player.AimAngleRad)) * lookLen;

            var w = _player.GetWeaponData();
            g.Restore(state); // HUD рисуем в координатах экрана

            // HP бары врагов
            foreach (var e in _enemies)
            {
                if (e.IsDead) continue;

                var s = WorldToScreen(e.Pos);

                // смещение вверх над спрайтом
                float spriteHpx = GameArt.FrameH * 2.0f * Zoom;
                float yOff = spriteHpx * 0.55f + 10f;

                float t = e.Stats.Hp / e.Stats.MaxHp;
                DrawHpBarScreen(g, s.X, s.Y - yOff, t);
            }


            // BOSS HP BAR
            if (_boss != null && !_boss.IsDead)
            {
                float t = _boss.Stats.Hp / _boss.Stats.MaxHp;
                int bossScale = 2;
                int barW = 420 * bossScale;
                int barH = 16 * bossScale;
                int x = 20;
                int y = client.Height - 40;

                var bg = new Rectangle(x, y, barW, barH);
                var fg = new Rectangle(x, y, (int)(barW * MathF.Max(0, MathF.Min(1, t))), barH);
                string bossname = _boss.Type.ToString();
                switch (_boss.Type)
                {
                    case BossType.GraveBrute: bossname = "Могильный кошмар"; break;
                    case BossType.BoneLord: bossname = "Костяной лорд"; break;
                    case BossType.WebQueen: bossname = "Паучья королева"; break;
                }
                g.FillRectangle(Brushes.Black, bg);
                g.FillRectangle(Brushes.Red, fg);
                g.DrawRectangle(Pens.White, bg);
                g.DrawString($"Босс: {bossname}  {(int)_boss.Stats.Hp}/{(int)_boss.Stats.MaxHp}",
                    new Font("Segoe UI", 12, FontStyle.Bold), Brushes.White, x, y - 10);
            }

            // HUD
            string weaponName = w.Name;
            string slow = _player.SlowFactor < 0.99f ? $" | SLOW {(_player.SlowTimer):0.0}s" : "";
            g.DrawString($"HP: {(int)_player.Stats.Hp}/{(int)_player.Stats.MaxHp} | Врагов: {_enemies.Count} | Оружие: {weaponName} (1-4){slow} | I: инвентарь",
                new Font("Segoe UI", 12, FontStyle.Bold), Brushes.White, 10, 10);

            if (!string.IsNullOrEmpty(_toast))
                g.DrawString(_toast, new Font("Segoe UI", 12, FontStyle.Bold), Brushes.White, 10, 40);

            // inventory overlay
            if (_inventoryOpen)
            {
                using (var dim = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                    g.FillRectangle(dim, 0, 0, client.Width, client.Height);

                // панель по центру, больше размер
                int pw = Math.Min(980, client.Width - 80);
                int ph = Math.Min(620, client.Height - 80);
                int px = (client.Width - pw) / 2;
                int py = (client.Height - ph) / 2;

                var panel = new Rectangle(px, py, pw, ph);

                using (var bg = new SolidBrush(Color.FromArgb(235, 20, 20, 20)))
                    g.FillRectangle(bg, panel);
                g.DrawRectangle(Pens.White, panel);

                int x = panel.X + 24;
                int y = panel.Y + 18;

                var small = new Font("Segoe UI", 11, FontStyle.Regular);
                var bold = new Font("Segoe UI", 12, FontStyle.Bold);

                // Заголовок
                g.DrawString("ИНВЕНТАРЬ (I закрыть) | 1-9 экипировать предмет", bold, Brushes.White, x, y);
                y += 28;

                // Левая колонка: экипировка
                int colW = panel.Width / 2;
                int xEq = x;
                int yEq = y;

                string eqW = _player.Inv.EquippedWeaponItem?.Name ?? "(нет)";
                string eqA = _player.Inv.EquippedArmor?.Name ?? "(нет)";
                string eqX = _player.Inv.EquippedAccessory?.Name ?? "(нет)";

                g.DrawString("ЭКИПИРОВКА:", bold, Brushes.White, xEq, yEq);
                yEq += 22;
                g.DrawString($"Оружие: {eqW}", small, Brushes.White, xEq, yEq); yEq += 20;
                g.DrawString($"Броня:  {eqA}", small, Brushes.White, xEq, yEq); yEq += 20;
                g.DrawString($"Аксессуар: {eqX}", small, Brushes.White, xEq, yEq); yEq += 20;

                // Правая колонка: статы
                int xSt = panel.X + colW + 24;
                int ySt = y + 10;

                var wd = _player.GetWeaponData();
                g.DrawString("СТАТЫ:", bold, Brushes.White, xSt, ySt);
                ySt += 22;
                g.DrawString($"HP: {(int)_player.Stats.Hp}/{(int)_player.Stats.MaxHp}", small, Brushes.White, xSt, ySt); ySt += 20;
                g.DrawString($"Броня: {_player.Stats.Armor:0.0}", small, Brushes.White, xSt, ySt); ySt += 20;
                g.DrawString($"Скорость: {_player.Stats.Speed:0}", small, Brushes.White, xSt, ySt); ySt += 20;
                g.DrawString($"Урон: {wd.Damage:0.0}", small, Brushes.White, xSt, ySt); ySt += 20;
                g.DrawString($"Скорость атаки: {wd.AttackSpeed:0.00}/с", small, Brushes.White, xSt, ySt); ySt += 20;
                g.DrawString($"Дальность: {wd.Range:0}", small, Brushes.White, xSt, ySt);

                y = Math.Max(yEq, ySt) + 20;
                g.DrawString("ПРЕДМЕТЫ:", bold, Brushes.White, x, y);
                y += 20;

                for (int i = 0; i < _player.Inv.Items.Count && i < 9; i++)
                {
                    var it = _player.Inv.Items[i];
                    g.DrawString($"{i + 1}) [{it.Slot}] {it.Name}", small, Brushes.White, x, y);
                    y += 20;
                }

            }

            if (_player.IsDead)
            {
                var big = new Font("Segoe UI", 12, FontStyle.Bold);
                var small = new Font("Segoe UI", 12, FontStyle.Bold);

                string t1 = "ВЫ ПОГИБЛИ";
                string t2 = "ESC — в меню";

                SizeF s1 = g.MeasureString(t1, big);
                SizeF s2 = g.MeasureString(t2, small);

                float q1 = (client.Width - s1.Width) / 2f;
                float w1 = (client.Height - s1.Height) / 2f - 20;

                float x2 = (client.Width - s2.Width) / 2f;
                float y2 = y1 + s1.Height + 10;

                g.DrawString(t1, big, Brushes.DarkRed, q1, w1);
                g.DrawString(t2, small, Brushes.DarkRed, x2, y2);
            }

            if (_levelWon)
            {
                g.DrawString("ПОБЕДА! Нажми ESC, чтобы выйти в меню",
                    new Font("Segoe UI", 12, FontStyle.Bold),
                    Brushes.Gold, 220, 300);
            }
        }

        private static void DrawHpBar(Graphics g, Vector2 pos, float t, int w, int h)
        {
            t = MathF.Max(0, MathF.Min(1, t));
            var bg = new Rectangle((int)(pos.X - w / 2), (int)(pos.Y - 28), w, h);
            var fg = new Rectangle(bg.X, bg.Y, (int)(w * t), h);
            g.FillRectangle(Brushes.Black, bg);
            g.FillRectangle(Brushes.LimeGreen, fg);
            g.DrawRectangle(Pens.White, bg);
        }

        private void DrawRotatedSingle(Graphics g, SpriteSheet sheet, Vector2 worldCenter, float worldW, float worldH, float angleRad, float angleOffsetRad = 0f)
        {
            if (!sheet.Loaded || sheet.Image == null) return;

            //экранные (учитывая камеру и зум)
            float sx = (worldCenter.X - _camTopLeft.X) * Zoom;
            float sy = (worldCenter.Y - _camTopLeft.Y) * Zoom;

            float sw = worldW * Zoom; // размеры тоже учитывают зум
            float sh = worldH * Zoom;

            var st = g.Save();

            // рисуем в экранных координатах
            g.ResetTransform();
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

            g.TranslateTransform(sx, sy);
            g.RotateTransform((angleRad + angleOffsetRad) * 180f / MathF.PI);

            var dst = new RectangleF(-sw / 2f, -sh / 2f, sw, sh);
            g.DrawImage(sheet.Image, dst, sheet.Src(0, 0), GraphicsUnit.Pixel);

            g.Restore(st); // возвращаем камеру обратно
        }


    }
}