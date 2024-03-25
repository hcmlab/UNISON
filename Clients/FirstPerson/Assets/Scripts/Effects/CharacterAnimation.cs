using UnityEngine;

namespace Clients
{
    public class CharacterAnimation : MonoBehaviour
    {
        public SkinnedMeshRenderer meshRenderer;
        public GameObject hospitalBed;
        private GameObject instance;

        public void MoveToHospitalBed()
        {
            instance = Instantiate(hospitalBed, transform);
            instance.transform.localPosition = new Vector3(.0f, 6.21f, -3.7f);
            instance.transform.localRotation = Quaternion.Euler(-90, 0, 180);
            instance.transform.localScale = new Vector3(1, 1, 1);

            transform.localPosition = new Vector3(.0f, 0.425f, .0f);
            transform.localRotation = Quaternion.Euler(-90, 0, 0);

            //cam 33, -120, 0
            //Vector3(1.26963055, 0.879747391, -0.0875375271)

            meshRenderer.SetBlendShapeWeight(0, 100.0f);
        }

        public void RecoverFromHospital()
        {
            transform.localPosition = new Vector3(.0f, .0f, .0f);
            transform.localRotation = Quaternion.Euler(0, 0, 0);

            Destroy(instance);
            meshRenderer.SetBlendShapeWeight(0, .0f);
        }
    }
}