import { StatsBlock } from '@components/stats'
import { ConditionsBlock, StatesBlock } from '@components/states'
import {
  ActionTemplatesBlock,
  SkillTemplatesBlock,
} from '@components/activities'
import LifeBlock from '@components/life/LifeBlock'
import { GraphStateBlock } from '@components/graph'
import { useAppSelector } from '@app/hooks'
import { GearBlock } from '@components/gear'
import { TimeBlock } from '@components/time'
import { Stack } from '@chakra-ui/react'

//import { isEncounterTime } from '../app/utils/is-encounter-time'

export default function CharacterSheet() {
  const { hands, wearing } = useAppSelector((state) => state.gear)

  return (
    <Stack
      margin={'0 auto'}
      width={'100%'}
      maxW={{ xl: '1200px' }}
      justifyContent="space-between"
      alignItems={'center'}
    >
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
    </Stack>
  )
}
