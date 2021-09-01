['http_proxy', 'https_proxy', 'no_proxy'].forEach((name) => {
  if (process.env[name]) process.env[name.toUpperCase()] = process.env[name]
})
process.env.GLOBAL_AGENT_ENVIRONMENT_VARIABLE_NAMESPACE = ''
require('./Build/node_modules/global-agent/bootstrap')
