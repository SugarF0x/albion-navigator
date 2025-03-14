const { roadinatorData, data, zoneNames } = require('./zones-data.js')
const { writeFileSync } = require('fs')

const portalNames = zoneNames.filter(e => e.includes('-'))

const zoneTypeMap = {
  STARTINGCITY: 0,
  CITY: 1,
  SAFEAREA: 2,
  YELLOW: 3,
  RED: 4,
  BLACK: 5,
  _ROAD_: 6
}

const nodes = data.nodes

const toSnakeCase = (val) => val
  .replace(/'/g,'')
  .replace(/\W+/g, ' ')
  .split(/ |\B(?=[A-Z])/)
  .map(word => word.toUpperCase())
  .join('_')

const match = ['SAFEAREA', 'OPENPVP_YELLOW', 'OPENPVP_RED', 'PLAYERCITY_BLACK', 'PLAYERCITY_BLACK_ROYAL', 'STARTINGCITY']
const include = ['PLAYERCITY_SAFEAREA_0', 'OPENPVP_BLACK', 'PLAYERCITY_BLACK_PORTALCITY']
const exclude = ['BRECILIEN']
const validNodes = nodes.filter(e => {
  const { enabled, type, displayname } = e._attr
  if (String(enabled) === 'false') return false
  if (exclude.some(m => toSnakeCase(displayname).includes(m))) return false
  if (match.some(m => type === m)) return true
  if (include.some(m => type.includes(m))) return true
  return false
})

const formattedZones = validNodes.map(e => ({
  id: Number(e._attr.id),
  displayName: e._attr.displayname,
  position: [e.x, -e.y],
  type: (() => {
    for (const [type, index] of Object.entries(zoneTypeMap))
      if (e._attr.type.includes(type)) return index
    return -1
  })(),
  connections: Array.from(new Set(e.exits.map(({ _attr: { targetid } }) => Number(targetid.split('@')[1]))))
}))

const uniqueIds = new Set()
formattedZones.forEach(item => uniqueIds.add(item.id))

const portalZoneNameToEntity = new Map()
for (const zone of formattedZones) {
  zone.connections = zone.connections.filter(exit => uniqueIds.has(exit))
  if (zone.displayName.includes('Portal')) portalZoneNameToEntity.set(zone.displayName, zone)
}

$: for (const zone of formattedZones) {
  for (const portalName of portalZoneNameToEntity.keys()) {
    const lookupName = portalName.replace(' Portal', '')
    if (zone.displayName === lookupName) {
      const entity = portalZoneNameToEntity.get(portalName)
      if (!entity) throw new Error('')
      zone.connections.push(entity.id)
      entity.connections.push(zone.id)
      portalZoneNameToEntity.delete(portalName)
      continue $
    }
  }
}

formattedZones.sort((a, b) => a.id - b.id)

for (let i = 0; i < formattedZones.length; i++) {
  const zone = formattedZones[i]
  const oldZoneId = zone.id
  const newZoneId = i
  zone.id = newZoneId
  zone.connections.forEach(connection => {
    const connectionZone = formattedZones.find(e => e.id === connection)
    if (!connectionZone) return
    const zoneIndex = connectionZone.connections.indexOf(oldZoneId)
    connectionZone.connections.splice(zoneIndex, 1, newZoneId)
  })
}

const indexShift = formattedZones.length
for (let i = 0; i < portalNames.length; i++) {
  const portalName = portalNames[i]
  formattedZones.push({
    id: indexShift + i,
    displayName: portalName,
    position: [0, 0],
    type: 6,
    connections: []
  })
}

const capitalizeFirst = str => str.charAt(0).toUpperCase() + str.slice(1)

const zoneLayerTypeToEnumMap = Array.from(new Set(roadinatorData.flatMap(e => [e.data.type])))
const zoneComponentTypeToEnumMap = Array.from(new Set(roadinatorData.flatMap(e => e.data.components.map(a => capitalizeFirst(a.type)))))

const colorToHashMap = {
  '': 'ffffff00',
  'Green': '00ff00ff',
  'Blue': '0000ffff',
  'Gold': 'ffd700ff',
}

const sizeToPropertyEnumHashMap = [
  'Small',
  'Big',
  'KPR',
  'HER',
  'MOR',
  'UND',
  'AVA',
]

const components = roadinatorData.flatMap(e => e.data.components)
const dedupedData = Array.from(new Set(components.map(JSON.stringify))).map(JSON.parse)
const componentsData = dedupedData.map((e, i) => {
  const properties = e.size ? [sizeToPropertyEnumHashMap.indexOf(e.size)] : []
  switch (e.bgcolor) {
    case 'Green': properties.push(7); break
    case 'Blue': properties.push(8); break
    case 'Gold': properties.push(9); break
  }

  const Type = (() => {
    const match = capitalizeFirst(e.type)
    switch (match) {
      case 'Dungeon':
      case 'Chest':
      case 'MistsCity': return zoneComponentTypeToEnumMap.indexOf(capitalizeFirst(e.type))
      default:
        const resources = match.match(/[A-Z][a-z]*/g) || []
        resources.forEach(resource => {
          switch (resource) {
            case 'Ore': properties.push(10); break
            case 'Rock': properties.push(11); break
            case 'Wood': properties.push(12); break
            case 'Fiber': properties.push(13); break
            case 'Hide': properties.push(14); break
          }
        })
        return 2
    }
  })()

  return {
    Id: i,
    Type,
    Tier: parseInt(e.tier),
    Properties: properties,
    DisplayName: capitalizeFirst(e.type),
  }
})

const path = '/Users/antuzov/RiderProjects/albion-navigator/Resources/Zones/zoneData.json'
writeFileSync(path, JSON.stringify(formattedZones, null, 2), 'utf-8')
