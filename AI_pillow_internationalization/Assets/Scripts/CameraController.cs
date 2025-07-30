using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("ターゲット設定")]
    [Tooltip("カメラが注視する対象オブジェクト")]
    public Transform target;

    [Header("カメラの基本設定")]
    [Tooltip("ターゲットからの距離")]
    public float distance = 12.5f;
    [Tooltip("最終的なカメラの上下の角度")]
    public float finalVerticalAngle = 50.0f;

    [Header("往復回転の設定")]
    [Tooltip("Y軸周りの自動回転速度")]
    public float rotationSpeed = 20.0f;
    [Tooltip("水平回転の最小角度")]
    public float minHorizontalAngle = -80.0f;
    [Tooltip("水平回転の最大角度")]
    public float maxHorizontalAngle = 80.0f;

    [Header("開始アニメーションの設定")]
    [Tooltip("正面で待機する時間（秒）")]
    public float initialWaitTime = 1.5f;
    [Tooltip("上昇にかかる時間（秒）")]
    public float ascendDuration = 1.5f;
    [Tooltip("正面から見るときの上下の角度")]
    public float initialVerticalAngle = 0.0f;

    private enum CameraState { Intro, Rotating }
    private CameraState _currentState = CameraState.Intro;

    private float _currentYRotation = 0.0f;
    private float _currentRotationSpeed;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("ターゲットが設定されていません。");
            this.enabled = false;
            return;
        }

        StartCoroutine(PlayIntroAnimation());
    }

    // 開始時のアニメーションを再生する
    private IEnumerator PlayIntroAnimation()
    {
        // 正面からの視点で待機
        // 角度を計算
        Quaternion initialRotation = Quaternion.Euler(initialVerticalAngle, 0, 0);
        Vector3 initialPosition = initialRotation * new Vector3(0, 0, -distance) + target.position;

        // カメラを初期位置に設定
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // 指定時間待機
        yield return new WaitForSeconds(initialWaitTime);

        // カメラの上昇アニメーション
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        // 最終的な位置と回転を計算
        Quaternion finalRotation = Quaternion.Euler(finalVerticalAngle, 0, 0);
        Vector3 finalPosition = finalRotation * new Vector3(0, 0, -distance) + target.position;

        while (elapsedTime < ascendDuration)
        {
            // 経過時間から補間係数（0から1）を計算
            float t = elapsedTime / ascendDuration;
            // 滑らかな動きにするためのイージング
            t = t * t * (3f - 2f * t);

            // 位置と回転を補間
            transform.position = Vector3.Lerp(startPosition, finalPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, finalRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = finalPosition;
        transform.rotation = finalRotation;

        // 往復回転へ移行
        _currentState = CameraState.Rotating;
        _currentYRotation = 0; // Y軸回転をリセット
        _currentRotationSpeed = rotationSpeed; // 回転速度を初期化
    }

    void LateUpdate()
    {
        // 往復回転の時のみ、カメラを動かす
        if (_currentState == CameraState.Rotating)
        {
            // Y軸回転角度を更新
            _currentYRotation += _currentRotationSpeed * Time.deltaTime;

            // 角度が範囲を超えたら、回転方向を反転
            if (_currentYRotation > maxHorizontalAngle || _currentYRotation < minHorizontalAngle)
            {
                _currentRotationSpeed = -_currentRotationSpeed;
                _currentYRotation = Mathf.Clamp(_currentYRotation, minHorizontalAngle, maxHorizontalAngle);
            }

            // 回転を計算
            Quaternion rotation = Quaternion.Euler(finalVerticalAngle, _currentYRotation, 0);

            // 位置を計算して適用
            transform.position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;
            transform.rotation = rotation;
        }
    }
}