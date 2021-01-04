module.exports = {
  title: "CanalSharp Document",
  description: "CanalSharp Document",
  markdown: {
    lineNumbers: true,
  },
  head: [
    ['link', { rel: 'icon', href: '/assets/canal.png' }]
  ],
  locales: {
    '/': {
      lang: 'English', 
      title: 'CanalSharp Document',
      description: 'CanalSharp Document'
    },
    '/zh/': {
      lang: '中文',
      title: 'CanalSharp 文档',
      description: 'CanalSharp 文档'
    }
  },
  themeConfig: {
    logo: '/assets/canal.png',
    smoothScroll: true,
    repo: 'dotnetcore/CanalSharp',
    docsDir: 'docs',
    editLinks: true,
    sidebarDepth: 2,
    locales: {
      '/': {
        selectText: 'Languages',
        label: 'English',
        ariaLabel: 'Languages',
        editLinkText: 'Edit this page on GitHub',
        serviceWorker: {
          updatePopup: {
            message: "New content is available.",
            buttonText: "Refresh"
          }
        },
        nav: [
          { text: 'Nested', link: '/quick-start', ariaLabel: 'Nested' }
        ],
        sidebar: [
          '/'
        ]
      },
      '/zh/': {
        selectText: '选择语言',
        label: '简体中文',
        editLinkText: '在 GitHub 上编辑此页',
        serviceWorker: {
          updatePopup: {
            message: "发现新内容可用.",
            buttonText: "刷新"
          }
        },
        nav: [
          { text: '快速入门', link: '/zh/quick-start' }
        ],
        sidebar: [
          '/zh/',
          {
            title: '入门',   // 必要的
            children: [
              '/zh/db-cfg',
              '/zh/canal-cfg',
              '/zh/quick-start',
            ]
          },
          {
            title: '进阶',
            children: [
              '/zh/parsing-data',
              '/zh/ack',
              '/zh/ha',
              '/zh/subscribe',
            ],
          }
        ]
      }
    }
  }
};
