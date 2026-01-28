using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace RogueLike_PROJECT
{
    public sealed class Sprite
    {
        public Bitmap? Image;
        public string DebugPath = "";
        public bool Loaded => Image != null;

        public Sprite(string relativePath)
        {
            string full = Path.Combine(AppContext.BaseDirectory, relativePath);
            DebugPath = full;
            if (File.Exists(full)) Image = new Bitmap(full);
        }
    }
    public enum Dir4 { Down = 0, Left = 1, Right = 2, Up = 3 }
    public enum AnimState { Idle, Walk, Roll, Death }

    public sealed class SpriteSheet
    {
        public Bitmap? Image;
        public int FrameW, FrameH;
        public int Cols, Rows;

        public bool Loaded => Image != null;

        public SpriteSheet(string relativePath, int frameW, int frameH, int cols, int rows)
        {
            FrameW = frameW;
            FrameH = frameH;
            Cols = cols;
            Rows = rows;

            string full = Path.Combine(AppContext.BaseDirectory, relativePath);
            if (File.Exists(full))
                Image = new Bitmap(full);
            else
                Image = null; 
        }

        public Rectangle Src(int col, int row)
            => new Rectangle(col * FrameW, row * FrameH, FrameW, FrameH);
    }

    public sealed class Animator
    {
        public AnimState State = AnimState.Idle;
        public float Time;
        public int Frame;

        public void Play(AnimState st)
        {
            if (State == st) return;
            State = st;
            Time = 0;
            Frame = 0;
        }

        public void Update(float dt, int frameCount, float fps, bool loop)
        {
            if (frameCount <= 1) { Frame = 0; return; }

            Time += dt;
            int f = (int)(Time * fps);

            if (loop) Frame = f % frameCount;
            else Frame = Math.Min(frameCount - 1, f);
        }

        public bool Finished(int frameCount, float fps)
        {
            if (frameCount <= 1) return true;
            return Time * fps >= frameCount - 0.001f;
        }
    }

    public static class GameArt
    {
        public const int FrameW = 32;
        public const int FrameH = 32;

        // Player
        public static SpriteSheet PlayerIdle = null!;
        public static SpriteSheet PlayerWalk = null!;
        public static SpriteSheet PlayerAttack = null!;
        public static SpriteSheet PlayerRoll = null!;
        public static SpriteSheet PlayerDeath = null!;

        // Weapons
        public static SpriteSheet WeaponSword = null!;
        public static SpriteSheet WeaponAxe = null!;
        public static SpriteSheet WeaponSpear = null!;
        public static SpriteSheet WeaponBow = null!;
        public static SpriteSheet Arrow = null!;
        public static SpriteSheet Web = null!;

        // Objects
        public static SpriteSheet Chest = null!;
        public static SpriteSheet Floor = null!;
        public static SpriteSheet Wall = null!;


        // Enemies
        public static SpriteSheet ZombieWalk = null!;
        public static SpriteSheet SkeletonWalk = null!;
        public static SpriteSheet SpiderWalk = null!;

        // Boss
        public static Sprite BossGraveBrute = null!;
        public static Sprite BossBoneLord = null!;
        public static Sprite BossWebQueen = null!;


        public static bool IsLoaded { get; private set; }

        public static void LoadOnce()
        {
            if (IsLoaded) return;

            PlayerIdle = new SpriteSheet(@"Assets\player_idle.png", FrameW, FrameH, cols: 1, rows: 1);
            PlayerWalk = new SpriteSheet(@"Assets\player_walk.png", FrameW, FrameH, cols: 6, rows: 4);
            PlayerRoll = new SpriteSheet(@"Assets\player_roll.png", FrameW, FrameH, cols: 4, rows: 4);
            PlayerDeath = new SpriteSheet(@"Assets\player_death.png", FrameW, FrameH, cols: 1, rows: 1);

            WeaponSword = new SpriteSheet(@"Assets\sword.png", FrameW, FrameH, cols: 1, rows: 1);
            WeaponAxe = new SpriteSheet(@"Assets\axe.png", FrameW, FrameH, cols: 1, rows: 1);
            WeaponSpear = new SpriteSheet(@"Assets\spear.png", FrameW, FrameH, cols: 1, rows: 1);
            WeaponBow = new SpriteSheet(@"Assets\bow.png", FrameW, FrameH, cols: 1, rows: 1);
            Arrow = new SpriteSheet(@"Assets\arrow.png", FrameW, FrameH, cols: 1, rows: 1);
            Web = new SpriteSheet(@"Assets\web.png", FrameW, FrameH, cols: 1, rows: 1);

            Chest = new SpriteSheet(@"Assets\chest.png", FrameW, FrameH, cols: 1, rows: 1);
            Floor = new SpriteSheet(@"Assets\floor.png", FrameW, FrameH, cols: 1, rows: 1);
            Wall = new SpriteSheet(@"Assets\wall.png", FrameW, FrameH, cols: 1, rows: 1);

            ZombieWalk = new SpriteSheet(@"Assets\zombie_walk.png", FrameW, FrameH, cols: 6, rows: 4);
            SkeletonWalk = new SpriteSheet(@"Assets\skeleton_walk.png", FrameW, FrameH, cols: 6, rows: 4);
            SpiderWalk = new SpriteSheet(@"Assets\spider_walk.png", FrameW, FrameH, cols: 4, rows: 4);

            IsLoaded = true;
        }

        public static Dir4 DirFromAngle(float angleRad)
        {
            float a = angleRad;

            float ax = MathF.Cos(a);
            float ay = MathF.Sin(a);

            if (MathF.Abs(ax) > MathF.Abs(ay))
                return ax >= 0 ? Dir4.Right : Dir4.Left;
            else
                return ay >= 0 ? Dir4.Down : Dir4.Up;
        }

        public static void DrawSheetFrame(Graphics g, SpriteSheet sheet, int col, int row, RectangleF dst)
        {
            if (!sheet.Loaded || sheet.Image == null) return;
            g.DrawImage(sheet.Image, dst, sheet.Src(col, row), GraphicsUnit.Pixel);
        }
    }
}