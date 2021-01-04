using System;
using System.Collections.Generic;
using System.Text;

namespace TestContainers.Images
{
    public static class PullPolicy
    {
        /**
     * Convenience method for returning the {@link DefaultPullPolicy} default image pull policy
     * @return {@link ImagePullPolicy}
     */
        public static IImagePullPolicy DefaultPolicy()
        {
            return new DefaultPullPolicy();
        }

        /**
         * Convenience method for returning the {@link AlwaysPullPolicy} alwaysPull image pull policy
         * @return {@link ImagePullPolicy}
         */
        public static IImagePullPolicy AlwaysPull => new AlwaysPullPolicy();


        /**
         * Convenience method for returning an {@link AgeBasedPullPolicy} Age based image pull policy,
         * @return {@link ImagePullPolicy}
         */
        public static IImagePullPolicy AgeBased(TimeSpan maxAge)
        {
            return new AgeBasedPullPolicy(maxAge);
        }
    }
}
