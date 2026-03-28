"use client";

import Script from "next/script";

// Environment variables for tracking IDs
// Set these in Vercel environment variables:
//   NEXT_PUBLIC_GA_ID           — Google Analytics (e.g., G-XXXXXXXXXX)
//   NEXT_PUBLIC_CLARITY_ID      — Microsoft Clarity (e.g., abcdefghij)
//   NEXT_PUBLIC_LINKEDIN_ID     — LinkedIn Insight Tag partner ID
//   NEXT_PUBLIC_FB_PIXEL_ID     — Facebook/Meta Pixel ID

const GA_ID = process.env.NEXT_PUBLIC_GA_ID;
const CLARITY_ID = process.env.NEXT_PUBLIC_CLARITY_ID;
const LINKEDIN_ID = process.env.NEXT_PUBLIC_LINKEDIN_ID;
const FB_PIXEL_ID = process.env.NEXT_PUBLIC_FB_PIXEL_ID;

export function AnalyticsScripts() {
  return (
    <>
      {/* Google Analytics (GA4) */}
      {GA_ID && (
        <>
          <Script
            src={`https://www.googletagmanager.com/gtag/js?id=${GA_ID}`}
            strategy="afterInteractive"
          />
          <Script id="ga4-init" strategy="afterInteractive">
            {`
              window.dataLayer = window.dataLayer || [];
              function gtag(){dataLayer.push(arguments);}
              gtag('js', new Date());
              gtag('config', '${GA_ID}', {
                page_title: document.title,
                send_page_view: true
              });
            `}
          </Script>
        </>
      )}

      {/* Microsoft Clarity (free heatmaps + session recordings) */}
      {CLARITY_ID && (
        <Script id="clarity-init" strategy="afterInteractive">
          {`
            (function(c,l,a,r,i,t,y){
              c[a]=c[a]||function(){(c[a].q=c[a].q||[]).push(arguments)};
              t=l.createElement(r);t.async=1;t.src="https://www.clarity.ms/tag/"+i;
              y=l.getElementsByTagName(r)[0];y.parentNode.insertBefore(t,y);
            })(window,document,"clarity","script","${CLARITY_ID}");
          `}
        </Script>
      )}

      {/* LinkedIn Insight Tag */}
      {LINKEDIN_ID && (
        <Script id="linkedin-init" strategy="afterInteractive">
          {`
            _linkedin_partner_id = "${LINKEDIN_ID}";
            window._linkedin_data_partner_ids = window._linkedin_data_partner_ids || [];
            window._linkedin_data_partner_ids.push(_linkedin_partner_id);
            (function(l) {
              if (!l){window.lintrk = function(a,b){window.lintrk.q.push([a,b])};
              window.lintrk.q=[]}
              var s = document.getElementsByTagName("script")[0];
              var b = document.createElement("script");
              b.type = "text/javascript";b.async = true;
              b.src = "https://snap.licdn.com/li.lms-analytics/insight.min.js";
              s.parentNode.insertBefore(b, s);})(window.lintrk);
          `}
        </Script>
      )}

      {/* Facebook/Meta Pixel */}
      {FB_PIXEL_ID && (
        <Script id="fb-pixel-init" strategy="afterInteractive">
          {`
            !function(f,b,e,v,n,t,s)
            {if(f.fbq)return;n=f.fbq=function(){n.callMethod?
            n.callMethod.apply(n,arguments):n.queue.push(arguments)};
            if(!f._fbq)f._fbq=n;n.push=n;n.loaded=!0;n.version='2.0';
            n.queue=[];t=b.createElement(e);t.async=!0;
            t.src=v;s=b.getElementsByTagName(e)[0];
            s.parentNode.insertBefore(t,s)}(window, document,'script',
            'https://connect.facebook.net/en_US/fbevents.js');
            fbq('init', '${FB_PIXEL_ID}');
            fbq('track', 'PageView');
          `}
        </Script>
      )}

      {/* Facebook Pixel noscript fallback */}
      {FB_PIXEL_ID && (
        <noscript>
          <img
            height="1"
            width="1"
            style={{ display: "none" }}
            src={`https://www.facebook.com/tr?id=${FB_PIXEL_ID}&ev=PageView&noscript=1`}
            alt=""
          />
        </noscript>
      )}
    </>
  );
}

// Custom event tracking helpers
export function trackEvent(eventName: string, params?: Record<string, string | number | boolean>) {
  // GA4
  if (typeof window !== "undefined" && GA_ID && (window as Record<string, unknown>).gtag) {
    (window as Record<string, (...args: unknown[]) => void>).gtag("event", eventName, params);
  }
  // LinkedIn
  if (typeof window !== "undefined" && LINKEDIN_ID && (window as Record<string, unknown>).lintrk) {
    (window as Record<string, (...args: unknown[]) => void>).lintrk("track", { conversion_id: eventName });
  }
  // Facebook
  if (typeof window !== "undefined" && FB_PIXEL_ID && (window as Record<string, unknown>).fbq) {
    (window as Record<string, (...args: unknown[]) => void>).fbq("trackCustom", eventName, params);
  }
}
