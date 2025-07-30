using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("�^�[�Q�b�g�ݒ�")]
    [Tooltip("�J��������������ΏۃI�u�W�F�N�g")]
    public Transform target;

    [Header("�J�����̊�{�ݒ�")]
    [Tooltip("�^�[�Q�b�g����̋���")]
    public float distance = 12.5f;
    [Tooltip("�ŏI�I�ȃJ�����̏㉺�̊p�x")]
    public float finalVerticalAngle = 50.0f;

    [Header("������]�̐ݒ�")]
    [Tooltip("Y������̎�����]���x")]
    public float rotationSpeed = 20.0f;
    [Tooltip("������]�̍ŏ��p�x")]
    public float minHorizontalAngle = -80.0f;
    [Tooltip("������]�̍ő�p�x")]
    public float maxHorizontalAngle = 80.0f;

    [Header("�J�n�A�j���[�V�����̐ݒ�")]
    [Tooltip("���ʂőҋ@���鎞�ԁi�b�j")]
    public float initialWaitTime = 1.5f;
    [Tooltip("�㏸�ɂ����鎞�ԁi�b�j")]
    public float ascendDuration = 1.5f;
    [Tooltip("���ʂ��猩��Ƃ��̏㉺�̊p�x")]
    public float initialVerticalAngle = 0.0f;

    private enum CameraState { Intro, Rotating }
    private CameraState _currentState = CameraState.Intro;

    private float _currentYRotation = 0.0f;
    private float _currentRotationSpeed;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("�^�[�Q�b�g���ݒ肳��Ă��܂���B");
            this.enabled = false;
            return;
        }

        StartCoroutine(PlayIntroAnimation());
    }

    // �J�n���̃A�j���[�V�������Đ�����
    private IEnumerator PlayIntroAnimation()
    {
        // ���ʂ���̎��_�őҋ@
        // �p�x���v�Z
        Quaternion initialRotation = Quaternion.Euler(initialVerticalAngle, 0, 0);
        Vector3 initialPosition = initialRotation * new Vector3(0, 0, -distance) + target.position;

        // �J�����������ʒu�ɐݒ�
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // �w�莞�ԑҋ@
        yield return new WaitForSeconds(initialWaitTime);

        // �J�����̏㏸�A�j���[�V����
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        // �ŏI�I�Ȉʒu�Ɖ�]���v�Z
        Quaternion finalRotation = Quaternion.Euler(finalVerticalAngle, 0, 0);
        Vector3 finalPosition = finalRotation * new Vector3(0, 0, -distance) + target.position;

        while (elapsedTime < ascendDuration)
        {
            // �o�ߎ��Ԃ����ԌW���i0����1�j���v�Z
            float t = elapsedTime / ascendDuration;
            // ���炩�ȓ����ɂ��邽�߂̃C�[�W���O
            t = t * t * (3f - 2f * t);

            // �ʒu�Ɖ�]����
            transform.position = Vector3.Lerp(startPosition, finalPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, finalRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = finalPosition;
        transform.rotation = finalRotation;

        // ������]�ֈڍs
        _currentState = CameraState.Rotating;
        _currentYRotation = 0; // Y����]�����Z�b�g
        _currentRotationSpeed = rotationSpeed; // ��]���x��������
    }

    void LateUpdate()
    {
        // ������]�̎��̂݁A�J�����𓮂���
        if (_currentState == CameraState.Rotating)
        {
            // Y����]�p�x���X�V
            _currentYRotation += _currentRotationSpeed * Time.deltaTime;

            // �p�x���͈͂𒴂�����A��]�����𔽓]
            if (_currentYRotation > maxHorizontalAngle || _currentYRotation < minHorizontalAngle)
            {
                _currentRotationSpeed = -_currentRotationSpeed;
                _currentYRotation = Mathf.Clamp(_currentYRotation, minHorizontalAngle, maxHorizontalAngle);
            }

            // ��]���v�Z
            Quaternion rotation = Quaternion.Euler(finalVerticalAngle, _currentYRotation, 0);

            // �ʒu���v�Z���ēK�p
            transform.position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;
            transform.rotation = rotation;
        }
    }
}