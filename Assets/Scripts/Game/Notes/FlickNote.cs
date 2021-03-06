﻿using System;
using UnityEngine;

public class FlickNote : Note
{
    public bool IsFlicking { get; set; }
    public float FlickingStartTime { get; set; }
    public Vector2 FlickingStartPosition { get; set; }

    private float age;

    protected override NoteRenderer CreateRenderer()
    {
        return Game.Config.UseClassicStyle
            ? (NoteRenderer) new ClassicFlickNoteRenderer(this)
            : throw new NotSupportedException();
    }

    public override void OnTouch(Vector2 screenPos)
    {
        // This method should never be invoked!
        throw new InvalidOperationException();
    }

    public void StartFlicking(Vector2 screenPos)
    {
        if (!Game.State.IsPlaying || IsFlicking) return;
        IsFlicking = true;
        FlickingStartTime = Game.Time;
        FlickingStartPosition = screenPos;
    }

    public bool UpdateFingerPosition(Vector2 screenPos)
    {
        if (!Game.State.IsPlaying) return false;
        if (IsCleared) return true;
        var swipeVector = screenPos - FlickingStartPosition;
        // TODO: Consider rotation
        if (Math.Abs(swipeVector.x) > Game.camera.orthographicSize * 0.01f)
        {
            TryClear();
            return true;
        }
        return false;
    }

    public override void Collect()
    {
        if (IsCollected) return;
    
        IsFlicking = default;
        FlickingStartTime = default;
        FlickingStartPosition = default;
        age = default;
        base.Collect();
    }

    public override NoteGrade CalculateGrade()
    {
        if (ShouldMiss()) return NoteGrade.Miss;

        var grade = NoteGrade.None;
        var timeUntil = TimeUntilStart + JudgmentOffset;

        if (Game.State.Mode == GameMode.Practice)
        {
            if (timeUntil >= 0)
            {
                if (timeUntil < 0.800f) grade = NoteGrade.Great;
                if (timeUntil <= 0.200f) grade = NoteGrade.Perfect;
            }
            else
            {
                var timePassed = -timeUntil;
                if (timePassed < 0.300f) grade = NoteGrade.Great;
                if (timePassed <= 0.100f) grade = NoteGrade.Perfect;
            }
        }
        else
        {
            if (timeUntil >= 0)
            {
                if (timeUntil < 0.150f) grade = NoteGrade.Great; // 0.400
                if (timeUntil <= 0.060f) grade = NoteGrade.Perfect; // 0.120
            }
            else
            {
                var timePassed = -timeUntil;
                if (timePassed < 0.150f) grade = NoteGrade.Great;
                if (timePassed <= 0.060f) grade = NoteGrade.Perfect;
            }
        }
        return grade;
    }

    public override bool IsAutoEnabled()
    {
        return base.IsAutoEnabled() || Game.State.Mods.Contains(Mod.AutoFlick);
    }
    
}