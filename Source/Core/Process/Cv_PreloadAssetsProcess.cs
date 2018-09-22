using System.Threading;
using Caravel.Core.Resource;
using static Caravel.Core.Resource.Cv_ResourceManager;

namespace Caravel.Core.Process
{
    public class Cv_PreloadAssetsProcess <Resource> : Cv_ParallelProcess where Resource : Cv_Resource, new()
    {
        private string m_sAssetExpression;
        private string m_sBundle;
        private LoadProgressDelegate m_LoadProgressDelegate;

        public Cv_PreloadAssetsProcess(string assetExpression, string bundle, LoadProgressDelegate loadProgressDelegate)
        {
            m_sAssetExpression = assetExpression;
            m_sBundle = bundle;
            m_LoadProgressDelegate = loadProgressDelegate;
        }

        protected internal override void VThreadFunction()
        {
            Cv_ResourceManager.Instance.Preload<Resource>(m_sAssetExpression, m_LoadProgressDelegate, m_sBundle);
            Succeed();
        }
    }
}