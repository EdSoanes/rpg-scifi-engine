import { PointInTime } from '../../lib/rpg-api/types'

export const isEncounterTime = (time?: PointInTime): boolean => {
  return !!time && (time.type === 'Turn' || time.type === 'EncounterBegins')
}
