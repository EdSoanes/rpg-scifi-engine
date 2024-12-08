import { PointInTime } from '../../lib/rpg-api/types'

export const isAfterEncounterTime = (time?: PointInTime): boolean => {
  return !!time && time.type === 'TimeEnds'
}
