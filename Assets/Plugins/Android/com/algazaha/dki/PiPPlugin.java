package com.algazaha.dki;

import android.app.PictureInPictureParams;
import android.content.res.Configuration;
import android.os.Build;
import android.os.Bundle;
import android.util.Rational;
import com.unity3d.player.UnityPlayerGameActivity;

public class PiPPlugin extends UnityPlayerGameActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    protected void onUserLeaveHint() {
        super.onUserLeaveHint();
        enterPiP();
    }

    private void enterPiP() {
        if (Build.VERSION.SDK_INT < 26) return;
        try {
            PictureInPictureParams params = new PictureInPictureParams
                .Builder()
                .setAspectRatio(new Rational(16, 9))
                .build();
            enterPictureInPictureMode(params);
        } catch (Exception e) {
            android.util.Log.e("PiP", "Erro: " + e.getMessage());
        }
    }

    // Chamado automaticamente pelo Android quando entra ou sai do PiP
    @Override
    public void onPictureInPictureModeChanged(boolean isInPictureInPictureMode, Configuration newConfig) {
        super.onPictureInPictureModeChanged(isInPictureInPictureMode, newConfig);

        if (mUnityPlayer != null) {
            if (isInPictureInPictureMode) {
                // Força o UnityPlayer a retomar a execução e a renderização imediata
                mUnityPlayer.resume();
            }
        }
    }

    public void triggerPiP() {
        runOnUiThread(this::enterPiP);
    }
}