import { Box, Flex, Heading } from '@chakra-ui/react'
import { PiCheckCircleFill, PiCheckCircleLight } from 'react-icons/pi'

import { SkillTemplate } from '@lib/rpg-api/cyborg-types'
import { StepperInput } from '@components/prop/stepper-input'
import { useState } from 'react'

import DescribePropertyDrawer from '@components/describe/DescribePropertyDrawer'

export declare interface SkillTemplatePanelProps {
  onSkillTemplate: (skillTemplate: SkillTemplate) => Promise<void>
  skillTemplate: SkillTemplate
}

function SkillTemplatePanel(props: SkillTemplatePanelProps) {
  const { skillTemplate } = props

  const [hide, setHide] = useState<boolean>(true)

  const onChange = (value: string) => {
    console.log('onChange', value)
  }

  return (
    <Flex gap={4} width={'100%'} alignItems={'center'} justifyItems={'center'}>
      {skillTemplate.isPerformable ? (
        <PiCheckCircleFill size={'16px'} />
      ) : (
        <PiCheckCircleLight size={'16px'} />
      )}
      <DescribePropertyDrawer
        entityId={skillTemplate.ownerId}
        prop={skillTemplate.ratingProp}
      >
        <Heading
          _hover={{ cursor: 'pointer' }}
          as={'h3'}
          color={'black'}
          size={'sm'}
        >
          {skillTemplate.name}
        </Heading>
      </DescribePropertyDrawer>
      <Box flex={1} justifyContent={'right'}>
        <StepperInput
          justifyContent={'right'}
          hideTriggers={hide}
          spinOnPress={false}
          size={'sm'}
          textStyle={'md'}
          value={String(skillTemplate.rating)}
          onValueChange={(e) => onChange(e.value)}
          onMouseOver={() => setHide(false)}
          onMouseLeave={() => setHide(true)}
        />
      </Box>
    </Flex>
  )
}

export default SkillTemplatePanel
