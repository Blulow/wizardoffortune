using System;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public static class Waiter
{
    public static async Task Seconds(float seconds, CancellationToken token = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            Application.exitCancellationToken, token);
        await Task.Delay(TimeSpan.FromSeconds(seconds), cts.Token);
    }

    public static async Task For(Tween tween, CancellationToken token = default)
    {
        await tween.AsyncWaitForCompletion();
        ThrowIfStopped(token);
    }

    public static void ThrowIfStopped(CancellationToken token = default)
    {
        Application.exitCancellationToken.ThrowIfCancellationRequested();
        token.ThrowIfCancellationRequested();
    }

    public static async Task Wait(Tween t, CancellationToken token = default)
    {
        if (t != null && t.IsActive()) await t.AsyncWaitForCompletion();
        Application.exitCancellationToken.ThrowIfCancellationRequested();
        token.ThrowIfCancellationRequested();
    }
}