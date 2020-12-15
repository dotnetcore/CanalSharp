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
      title: 'CanalSharp 中文文档',
      description: 'CanalSharp 中文文档'
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
          { text: 'Nested', link: '/get-start', ariaLabel: 'Nested' }
        ],
        sidebar: [
          '/',
          '/get-start'
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
          { text: '嵌套', link: '/zh/get-start' }
        ],
        sidebar: [
          {
            title: '入门',   // 必要的
            //path: '/foo/',      // 可选的, 标题的跳转链接，应为绝对路径且必须存在
            //collapsable: false, // 可选的, 默认值是 true,
            //sidebarDepth: 1,    // 可选的, 默认值是 1
            children: [
              '/zh/',
              '/zh/get-start'
            ]
          },
          {
            title: '进阶',
            children: [ /* ... */ ],
            initialOpenGroupIndex: -1 // 可选的, 默认值是 0
          }
        ]
      }
    }
  }
};
