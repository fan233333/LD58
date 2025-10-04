using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("��Ʒ����")]
    public string itemName = "δ������Ʒ"; // ��Ʒ����
    public ItemType itemType = ItemType.Circle; // ��Ʒ����
    public int value = 1; // ��Ʒ��ֵ

    [Header("�Ӿ�Ч��")]
    public Color itemColor = Color.white; // ��Ʒ��ɫ
    public ParticleSystem collectEffect; // �ռ���Ч

    [Header("��Ƶ")]
    public AudioClip collectSound; // �ռ���Ч

    // ��Ʒ����ö��
    public enum ItemType
    {
        Circle,     // Բ��
        Triangle,   // ������
        Square,     // ������
        Diamond,    // ����
        Star,       // ����
        Hexagon,    // ������
        Custom      // �Զ�������
    }

    // ��ȡ��Ʒ��������
    public string GetTypeName()
    {
        switch (itemType)
        {
            case ItemType.Circle: return "Բ��";
            case ItemType.Triangle: return "������";
            case ItemType.Square: return "������";
            case ItemType.Diamond: return "����";
            case ItemType.Star: return "����";
            case ItemType.Hexagon: return "������";
            case ItemType.Custom: return itemName;
            default: return "δ֪";
        }
    }

    // ��ȡ��Ʒ���ͱ�ʶ������ͳ�ƣ�
    public string GetTypeKey()
    {
        return itemType.ToString();
    }
}