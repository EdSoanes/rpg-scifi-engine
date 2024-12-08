import { Heading, Stack } from '@chakra-ui/react'
import { BodyPart } from '@lib/rpg-api/cyborg-types'

export interface BodyPartPanelProps {
  bodyPart?: BodyPart
}
function BodyPartPanel(props: BodyPartPanelProps) {
  const { bodyPart } = props
  return (
    <Stack direction={'column'}>
      <Heading size={'md'}>{bodyPart?.name}</Heading>{' '}
      <span>{bodyPart?.bodyPartType}</span>
      <li>
        {bodyPart?.injuries?.map((i) => <li key={i.id}>{i.severity}</li>)}
      </li>
    </Stack>
  )
}

export default BodyPartPanel
