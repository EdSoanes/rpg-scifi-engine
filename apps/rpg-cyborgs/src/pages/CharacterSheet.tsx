import { Heading } from '@chakra-ui/react'
import { StatsBlock } from '../components/stats'
import { ConditionsBlock, StatesBlock } from '../components/states'
import {
  ActionTemplatesBlock,
  SkillTemplatesBlock,
} from '../components/activities'
import LifeBlock from '../components/life/LifeBlock'
import { GraphStateBlock } from '../components/graph'
import { useSelector } from 'react-redux'
import { selectPlayerCharacter } from '../app/graphState/graphSelectors'
import { useAppSelector } from '../app/hooks'
import { GearBlock } from '../components/gear'
import { TimeBlock } from '../components/time'
import { StepperInput } from '../components/ui/stepper-input'
import { useState } from 'react'

//import { isEncounterTime } from '../app/utils/is-encounter-time'

export default function CharacterSheet() {
  //const time = useSelector(selectTime)
  const playerCharacter = useSelector(selectPlayerCharacter)
  const { hands, wearing } = useAppSelector((state) => state.gear)
  const [val, setVal] = useState<string>('10')
  return (
    <>
      <Heading>{playerCharacter?.name ?? 'Nobody'}</Heading>
      <StepperInput
        spinOnPress={false}
        maxW="200px"
        size={'md'}
        value={val}
        onValueChange={(e) => {
          setVal(e.value)
        }}
      />
      <TimeBlock />
      <StatsBlock />
      <StatesBlock />
      <ConditionsBlock />
      <ActionTemplatesBlock />
      <SkillTemplatesBlock />
      <LifeBlock />
      <GearBlock name={'Hands'} container={hands} />
      <GearBlock name={'Wearing'} container={wearing} />
      <GraphStateBlock />
    </>
  )
}
