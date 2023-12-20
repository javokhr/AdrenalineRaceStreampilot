using UnityEngine;

public class ContinuousRotation : MonoBehaviour
{
    public float rotationSpeed = 10f; // �������� �������� �������

    void FixedUpdate()
    {
        // �������� ������� ���� �������� �������
        Vector3 currentRotation = transform.rotation.eulerAngles;

        // ��������� � �������� ���� �������� ����� �������� ��� ��� Y
        float newYRotation = currentRotation.y + (rotationSpeed * Time.deltaTime);

        // ������� ����� ������ �������� � ����������� ��������� ��� ��� Y
        Quaternion newRotation = Quaternion.Euler(currentRotation.x, newYRotation, currentRotation.z);

        // ��������� ����� ������� � �������
        transform.rotation = newRotation;
    }
}
