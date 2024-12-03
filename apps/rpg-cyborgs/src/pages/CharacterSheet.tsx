import { Heading } from '@chakra-ui/react'
import { StatsBlock } from '../components/stats'
import { StatesBlock } from '../components/states'
import { ActionTemplatesBlock } from '../components/activities'
import LifeBlock from '../components/life/LifeBlock'
import { GraphStateBlock } from '../components/graph'
import { useSelector } from 'react-redux'
import { selectPlayerCharacter } from '../app/graphState/graphSelectors'
import { useAppSelector } from '../app/hooks'
import { GearBlock } from '../components/gear'
import { TimeBlock } from '../components/time'
//import { isEncounterTime } from '../app/utils/is-encounter-time'

export default function CharacterSheet() {
  //const time = useSelector(selectTime)
  const playerCharacter = useSelector(selectPlayerCharacter)
  const { hands, wearing } = useAppSelector((state) => state.gear)

  return (
    <>
      <Heading>{playerCharacter?.name ?? 'Nobody'}</Heading>
      <TimeBlock />
      <StatsBlock />
      <StatesBlock />
      <ActionTemplatesBlock />
      <LifeBlock />
      <GearBlock name={'Hands'} container={hands} />
      <GearBlock name={'Wearing'} container={wearing} />
      <GraphStateBlock />
    </>
  )
}
