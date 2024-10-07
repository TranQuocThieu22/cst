const PROXY_CONFIG = [
  {
    context: ["/"],
    // target: "https://cst.aqtech.vn",
    target: "http://localhost:54383/",
    // target: "https://cst.aqtech.vn:4244/#/",
    secure: false,
    changeOrigin: true,
    logLevel: "debug",
    headers: { host: "localhost" },
    timeout: 60000,
    cookieDomainRewrite: {
      localhost: "localhost",
    },
  },
];
module.exports = PROXY_CONFIG;
