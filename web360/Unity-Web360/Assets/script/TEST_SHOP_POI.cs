using UnityEngine;

public class TEST_SHOP_POI : MonoBehaviour
{
    public ShopShortInfo shortInfoPoi;
    public ShopInformaitions infoShopPoi;

    public void Inizialize(ShopShortInfo shortInfo)
    {
        shortInfoPoi = shortInfo;
    }

    public void SetInfo()
    {
        StartCoroutine(GETShops.GETExtendedInfo(shortInfoPoi, (info) =>
        {
            infoShopPoi = info;
            Debug.Log(infoShopPoi.ToString());
        }));
    }

}
